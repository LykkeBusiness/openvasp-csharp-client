using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using Timer = System.Timers.Timer;

namespace OpenVASP.CSharpClient.Internals.Services
{
    public class OutboundEnvelopeService: IOutboundEnvelopeService
    {
        private readonly IWhisperService _whisperRpc;
        private readonly double _envelopeExpiryInSeconds;
        private readonly int _messageRetries;
        private readonly ConcurrentDictionary<string, Timer> _timers;
        private readonly ILogger<OutboundEnvelopeService> _logger;
        
        public OutboundEnvelopeService(
            IWhisperService whisperRpc,
            double envelopeExpiryInSeconds,
            int messageRetries,
            ILoggerFactory loggerFactory)
        {
            _whisperRpc = whisperRpc;
            _envelopeExpiryInSeconds = envelopeExpiryInSeconds;
            _messageRetries = messageRetries;
            _timers = new ConcurrentDictionary<string, Timer>();
            _logger = loggerFactory?.CreateLogger<OutboundEnvelopeService>();
        }
        
        public event Func<OutboundEnvelopeReachedMaxResendsEvent, Task> OutboundEnvelopeReachedMaxResends;

        public async Task SendEnvelopeAsync(OutboundEnvelope outboundEnvelope, bool doRetries)
        {
            await _whisperRpc.SendMessageAsync(
                outboundEnvelope.Envelope.Topic,
                outboundEnvelope.Envelope.EncryptionKey,
                outboundEnvelope.Envelope.EncryptionType,
                outboundEnvelope.Payload);

            if (doRetries)
            {
                var timer = new Timer(_envelopeExpiryInSeconds*1000);
                timer.Elapsed += async ( sender, e ) => await ProcessedScheduledEnvelopeAsync(timer, outboundEnvelope);
                _timers[outboundEnvelope.Id] = timer;
                timer.Start();
            }
        }

        public async Task AcknowledgeAsync(MessageEnvelope messageEnvelope, string payload)
        {
            await _whisperRpc.SendMessageAsync(
                messageEnvelope.Topic,
                messageEnvelope.EncryptionKey,
                messageEnvelope.EncryptionType,
                payload);
        }

        public async Task RemoveQueuedEnvelopeAsync(string payloadEnvelopeAck)
        {
            _timers.TryGetValue(payloadEnvelopeAck, out var timer);
            timer?.Stop();
            timer?.Dispose();
            _timers.TryRemove(payloadEnvelopeAck, out _);
        }

        private async Task ProcessedScheduledEnvelopeAsync(Timer timer, OutboundEnvelope outboundEnvelope)
        {
            if (_messageRetries == outboundEnvelope.TotalResents)
            {
                _logger?.LogWarning(
                    $"Reached max resends for connectionId {outboundEnvelope.ConnectionId} on topic {outboundEnvelope.Envelope.Topic} ");

                await TriggerAsyncEvent(OutboundEnvelopeReachedMaxResends, new OutboundEnvelopeReachedMaxResendsEvent
                {
                    ConnectionId = outboundEnvelope.ConnectionId
                });

                await RemoveQueuedEnvelopeAsync(outboundEnvelope.Id);

                return;
            }

            await _whisperRpc.SendMessageAsync(
                outboundEnvelope.Envelope.Topic,
                outboundEnvelope.Envelope.EncryptionKey,
                outboundEnvelope.Envelope.EncryptionType,
                outboundEnvelope.Payload);

            outboundEnvelope.TotalResents += 1;
        }
        
        private Task TriggerAsyncEvent<T>(Func<T, Task> eventDelegates, T @event)
        {
            if (eventDelegates == null)
                return Task.CompletedTask;

            var tasks = eventDelegates.GetInvocationList()
                .OfType<Func<T, Task>>()
                .Select(d => d(@event));
            return Task.WhenAll(tasks);
        }
    }
}