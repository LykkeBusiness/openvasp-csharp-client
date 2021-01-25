using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using OpenVASP.CSharpClient.Internals.Cryptography;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Models;
using OpenVASP.CSharpClient.Internals.Utils;

namespace OpenVASP.CSharpClient.Internals.Services
{
    internal class TransportClient : ITransportService
    {
        private readonly string _vaspId;
        private string _filter;
        private readonly string _privateTransportKey;
        private readonly IWhisperService _whisperRpc;
        private readonly IVaspCodesService _vaspCodesService;
        private readonly IOutboundEnvelopeService _outboundEnvelopeService;
        private readonly ConcurrentDictionary<string, Connection> _connections;
        private readonly ConcurrentDictionary<string, OpenVaspPayloadBase> _openVaspPayloads;
        private readonly HashSet<string> _activeTopics;
        private readonly Timer _timer;
        private readonly ILogger<TransportClient> _logger;

        public TransportClient(
            string vaspId,
            string privateTransportKey,
            IWhisperService whisperRpc,
            IVaspCodesService vaspCodesService,
            IOutboundEnvelopeService outboundEnvelopeService,
            ILoggerFactory loggerFactory)
        {
            _vaspId = vaspId;
            _privateTransportKey = privateTransportKey;
            _whisperRpc = whisperRpc;
            _vaspCodesService = vaspCodesService;
            _outboundEnvelopeService = outboundEnvelopeService;
            _connections = new ConcurrentDictionary<string, Connection>();
            _openVaspPayloads = new ConcurrentDictionary<string, OpenVaspPayloadBase>();
            _activeTopics = new HashSet<string>();
            _logger = loggerFactory?.CreateLogger<TransportClient>();
            
            _outboundEnvelopeService.OutboundEnvelopeReachedMaxResends += OnOutboundEnvelopeReachedMaxResends;
            
            _timer = new Timer(5*100);
            _timer.Elapsed += async delegate { await TimerOnElapsed(); };
            
            StartAsync().GetAwaiter().GetResult();
        }

        private async Task TimerOnElapsed()
        {
            await CheckAllActiveAsync();
            await CheckPendingConnectionRequests();
        }

        public event Func<TransportMessageEvent, Task> TransportMessageReceived;

        private async Task StartAsync()
        {
            var transportKey = ECDH_Key.ImportKey(_privateTransportKey);
            var privateKeyId = await _whisperRpc.RegisterKeyPairAsync(transportKey.PrivateKey);
            var filter = await _whisperRpc.CreateMessageFilterAsync(_vaspId.Substring(4), privateKeyId);
            _filter = filter;
            _timer.Start();
        }
        
        public async Task<string> CreateConnectionAsync(string counterPartyVaspId)
        {
            var counterPartyVaspCode = counterPartyVaspId.Substring(4, 8);
            var vaspTransportKey = await _vaspCodesService.GetTransportKeyAsync(counterPartyVaspCode);
            if (vaspTransportKey == null)
            {
                throw new InvalidOperationException($"Couldn't get TransportKey for vasp code {counterPartyVaspCode}");
            }

            var sessionKey = ECDH_Key.GenerateKey();
            var topic = TopicGenerator.GenerateConnectionTopic();
            var privateKeyId = await _whisperRpc.RegisterKeyPairAsync(sessionKey.PrivateKey);
            var filter = await _whisperRpc.CreateMessageFilterAsync(topic, privateKeyId);
            _activeTopics.Add(topic);

            var connection = new Connection
            {
                Id = Guid.NewGuid().ToString("N"),
                Filter = filter,
                InboundTopic = topic,
                Status = ConnectionStatus.Active,
                CounterPartyVaspId = counterPartyVaspId,
                PrivateKey = sessionKey.PrivateKey,
            };

            _connections[connection.Id] = connection;

            return connection.Id;
        }
        
        public async Task SendAsync(string connectionId, string message, Instruction instruction, string receiverVaspId)
        {
            var connection = _connections[connectionId];
            
            var receiverVaspCode = (receiverVaspId ?? connection.CounterPartyVaspId).Substring(4, 8);

            var envelopeId = Guid.NewGuid().ToString("N");
            
            var payload = new OpenVaspPayload(
                instruction,
                _vaspId,
                connection.Id,
                envelopeId)
            {
                ReturnTopic = connection.InboundTopic,
                OvMessage = message
            };
            
            if (instruction == Instruction.Invite || instruction == Instruction.Accept || instruction == Instruction.Deny)
                payload.EcdhPk = ECDH_Key.ImportKey(connection.PrivateKey).PublicKey;
            
            var topic = connection.OutboundTopic ?? receiverVaspCode;
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidOperationException($"Topic is empty for connection {connection.Id}");
            
            var envelope = new MessageEnvelope
            {
                Topic = topic,
            };
            
            if (instruction == Instruction.Invite || instruction == Instruction.Close && string.IsNullOrWhiteSpace(connection.SymKeyId))
            {
                envelope.EncryptionType = EncryptionType.Asymmetric;

                var vaspTransportKey = await _vaspCodesService.GetTransportKeyAsync(receiverVaspCode);
                if (vaspTransportKey == null)
                {
                    throw new InvalidOperationException($"Transport key for vasp code {receiverVaspCode} cannot be found during message sending");
                }

                envelope.EncryptionKey = vaspTransportKey.DecompressPublicKey().ToHex(true);
            }
            else if (instruction == Instruction.Accept || instruction == Instruction.Deny
                                                       || instruction == Instruction.Close && connection.Status == ConnectionStatus.PartiallyActive)
            {
                envelope.EncryptionType = EncryptionType.Asymmetric;
                envelope.EncryptionKey = connection.CounterPartyPublicKey.DecompressPublicKey().ToHex(true);
            }
            else
            {
                envelope.EncryptionType = EncryptionType.Symmetric;
                envelope.EncryptionKey = connection.SymKeyId;
            }
            
            var outboundEnvelope = new OutboundEnvelope
            {
                Id = envelopeId,
                ConnectionId = connectionId,
                Envelope = envelope,
                TotalResents = 0,
                Payload = payload.ToString()
            };

            _openVaspPayloads[payload.EnvelopeId] = payload;

            await _outboundEnvelopeService.SendEnvelopeAsync(outboundEnvelope, instruction != Instruction.Deny);
            
            if (instruction == Instruction.Deny)
                await DeactivateAsync(connectionId);
        }
        
        private async Task CheckAllActiveAsync()
        {
            var activeConnections = _connections.Values.Where(x =>
                x.Status == ConnectionStatus.Active ||
                x.Status == ConnectionStatus.PartiallyActive);

            foreach (var connection in activeConnections)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(connection.Filter))
                    {
                        if (string.IsNullOrWhiteSpace(connection.SharedPrivateEncryptionKey))
                        {
                            var privateKeyId = await _whisperRpc.RegisterKeyPairAsync(connection.PrivateKey);
                            connection.Filter = await _whisperRpc.CreateMessageFilterAsync(connection.InboundTopic, privateKeyId);
                        }
                        else
                        {
                            (connection.Filter, connection.SymKeyId) = await RegisterConnectionAsync(connection.InboundTopic, connection.SharedPrivateEncryptionKey);
                        }
                    }

                    if (!_activeTopics.Contains(connection.InboundTopic))
                        _activeTopics.Add(connection.InboundTopic);

                    var messages = await _whisperRpc.GetMessagesAsync(connection.Filter);
                    foreach (var message in messages)
                    {
                        await ProcessReceivedMessageAsync(message.MessageEnvelope.Topic, message.Payload);
                    }
                }
                catch (RpcResponseException exception)
                {
                    if (exception.Message.ToLower() == "filter not found")
                    {
                        if (string.IsNullOrWhiteSpace(connection.SharedPrivateEncryptionKey))
                        {
                            if (string.IsNullOrWhiteSpace(connection.PrivateKey))
                            {
                                connection.Filter = await _whisperRpc.CreateMessageFilterAsync(connection.InboundTopic, symKeyId: connection.SymKeyId);
                            }
                            else
                            {
                                var privateKeyId = await _whisperRpc.RegisterKeyPairAsync(connection.PrivateKey);
                                connection.Filter = await _whisperRpc.CreateMessageFilterAsync(connection.InboundTopic, privateKeyId);
                            }
                        }
                        else
                        {
                            (connection.Filter, connection.SymKeyId) = await RegisterConnectionAsync(connection.InboundTopic, connection.SharedPrivateEncryptionKey);
                        }

                        _logger?.LogInformation($"New filter({connection.Filter}) was generated for vasp id {connection.CounterPartyVaspId} on connection '{connection.Id}'.");
                    }
                    else
                    {
                        _logger?.LogError(exception, "Failed to get new messages");
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Failed to get new messages for connection {connection.Id}");
                }
            }
        }
        
        private async Task CheckPendingConnectionRequests()
        {
                try
                {
                    var filterMessages = await _whisperRpc.GetMessagesAsync(_filter);

                    foreach (var message in filterMessages)
                    {
                        try
                        {
                            await ProcessReceivedMessageAsync(_vaspId.Substring(4), message.Payload);
                        }
                        catch (Exception e)
                        {
                            _logger?.LogError(e, $"Failed to get process messages for vasp {_vaspId}");
                        }
                    }
                }
                catch (RpcResponseException exception)
                {
                    if (exception.Message.ToLower() == "filter not found")
                    {
                        var privateKeyId = await _whisperRpc.RegisterKeyPairAsync(_privateTransportKey);
                        var messagesFilter = await _whisperRpc.CreateMessageFilterAsync(_vaspId.Substring(4, 8), privateKeyId);
                        _filter = messagesFilter;

                        _logger?.LogInformation($"New filter({messagesFilter}) was generated for the vasp id '{_vaspId}'.");
                    }
                    else
                    {
                        _logger?.LogError(exception, $"Failed to get process messages for vasp {_vaspId}");
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Failed to get process messages for vasp {_vaspId}");
                }
        }
        
        private async Task<(string, string)> RegisterConnectionAsync(string topic, string symKey)
        {
            var symKeyId = await _whisperRpc.RegisterSymKeyAsync(symKey);
            var filter = await _whisperRpc.CreateMessageFilterAsync(topic, symKeyId: symKeyId);

            return (filter, symKeyId);
        }
        
        private async Task DeactivateAsync(string connectionId)
        {
            var connection = _connections[connectionId];
            if (connection == null)
                throw new ArgumentException($"Connection with id '{connectionId}' was not found");

            connection.Status = ConnectionStatus.Passive;

            _activeTopics.Remove(connection.InboundTopic);
        }
        
        private async Task ProcessReceivedMessageAsync(
            string topic,
            string messagePayload)
        {
            var payload = OpenVaspPayload.Create(messagePayload);

            _logger?.LogInformation(
                $"Received {payload.Instruction} with topic {topic} for connection {payload.ConnectionId} from {payload.SenderVaspId} to {_vaspId}");

            switch (payload.Instruction)
            {
                case Instruction.Ack:
                    await HandleAcknowledgementMessageAsync(payload);
                    break;
                case Instruction.Invite:
                    await HandleInviteMessageAsync(payload);
                    break;
                case Instruction.Accept:
                    await HandleAcceptMessageAsync(payload);
                    break;
                case Instruction.Deny:
                    await HandleDenyMessageAsync(payload);
                    break;
                case Instruction.Update:
                    await HandleUpdateMessageAsync(payload);
                    break;
                case Instruction.Close:
                    await HandleCloseMessageAsync(payload);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!_openVaspPayloads.ContainsKey(payload.EnvelopeId))
            {
                _openVaspPayloads[payload.EnvelopeId] = payload;
            }
        }

        private async Task HandleAcknowledgementMessageAsync(OpenVaspPayload payload)
        {
            await _outboundEnvelopeService.RemoveQueuedEnvelopeAsync(payload.EnvelopeAck);

            var ackedMessage = _openVaspPayloads[payload.EnvelopeAck];

            if (ackedMessage?.Instruction == Instruction.Close)
                await DeactivateAsync(payload.ConnectionId);

            var connection = _connections[payload.ConnectionId];
            if (connection == null)
                throw new ArgumentException($"Connection with id '{payload.ConnectionId}' was not found");
            if (connection.Status == ConnectionStatus.PartiallyActive)
            {
                connection.Status = ConnectionStatus.Active;
            }
        }

        private Task HandleCloseMessageAsync(OpenVaspPayload payload)
        {
            return HandleMessageAsync(
                payload,
                true,
                c =>
                {
                    c.Status = ConnectionStatus.Passive;
                    return true;
                });
        }

        private Task HandleUpdateMessageAsync(OpenVaspPayload payload)
        {
            return HandleMessageAsync(
                payload,
                true,
                c =>
                {
                    if (c.Status != ConnectionStatus.PartiallyActive)
                        return false;

                    c.Status = ConnectionStatus.Active;
                    return true;
                });
        }

        private Task HandleDenyMessageAsync(OpenVaspPayload payload)
        {
            return HandleMessageAsync(
                payload,
                false,
                connectionUpdate: c =>
                {
                    c.Status = ConnectionStatus.Passive;
                    return true;
               });
        }

        private async Task HandleAcceptMessageAsync(OpenVaspPayload payload)
        {
            var connection = _connections[payload.ConnectionId];
            if (connection == null)
                throw new ArgumentException($"Connection with id '{payload.ConnectionId}' was not found");

            var sharedSecret = ECDH_Key.ImportKey(connection.PrivateKey).GenerateSharedSecretHex(payload.EcdhPk);
            (connection.Filter, connection.SymKeyId) = await RegisterConnectionAsync(connection.InboundTopic, sharedSecret);
            connection.SharedPrivateEncryptionKey = sharedSecret;
            connection.OutboundTopic = payload.ReturnTopic;

            await AcknowledgeMessageAsync(
                payload,
                connection.SymKeyId,
                null);

            var senderVaspCode = payload.SenderVaspId.Substring(4, 8);
            var signingKey = await _vaspCodesService.GetSigningKeyAsync(senderVaspCode);
            await TriggerAsyncEvent(TransportMessageReceived,
                new TransportMessageEvent
                {
                    ConnectionId = payload.ConnectionId,
                    SenderVaspId = payload.SenderVaspId,
                    Instruction = payload.Instruction,
                    Payload = payload.OvMessage,
                    Timestamp = DateTime.UtcNow,
                    SigningKey = signingKey
                });
        }

        private async Task HandleMessageAsync(
            OpenVaspPayload payload,
            bool doAcknowledge,
            Func<Connection, bool> connectionUpdate = null)
        {
            var connection = _connections[payload.ConnectionId];
            if (connection == null)
                throw new ArgumentException($"Connection with id '{payload.ConnectionId}' was not found");

            var originalConnStatus = connection.Status;

            connectionUpdate?.Invoke(connection);;

            if (doAcknowledge)
            {
                var symKeyId = originalConnStatus == ConnectionStatus.PartiallyActive ? null : connection.SymKeyId;
                string asymKey = null;
                if (symKeyId == null)
                {
                    asymKey = payload.EcdhPk
                        ?? connection.CounterPartyPublicKey
                        ?? await _vaspCodesService.GetTransportKeyAsync(connection.CounterPartyVaspId.Substring(4));
                    asymKey = asymKey?.DecompressPublicKey().ToHex(true);
                }
                if (string.IsNullOrWhiteSpace(symKeyId) && string.IsNullOrWhiteSpace(asymKey))
                    _logger?.LogWarning($"Can't sent ACK for {payload.Instruction} via connection {payload.ConnectionId}");
                else
                    await AcknowledgeMessageAsync(
                        payload,
                        symKeyId,
                        asymKey,
                        string.IsNullOrWhiteSpace(connection.SymKeyId) && string.IsNullOrWhiteSpace(payload.EcdhPk)
                            ? connection.CounterPartyVaspId.Substring(4)
                            : null);
            }

            var senderVaspCode = payload.SenderVaspId.Substring(4, 8);
            var signingKey = await _vaspCodesService.GetSigningKeyAsync(senderVaspCode);
            await TriggerAsyncEvent(TransportMessageReceived,
                new TransportMessageEvent
                {
                    ConnectionId = payload.ConnectionId,
                    SenderVaspId = payload.SenderVaspId,
                    Instruction = payload.Instruction,
                    Payload = payload.OvMessage,
                    Timestamp = DateTime.UtcNow,
                    SigningKey = signingKey
                });
        }

        private async Task HandleInviteMessageAsync(OpenVaspPayload payload)
        {
            var senderVaspCode = payload.SenderVaspId.Substring(4, 8);
            var vaspTransportKey = await _vaspCodesService.GetTransportKeyAsync(senderVaspCode);
            if (vaspTransportKey == null)
            {
                _logger?.LogError($"Transport key for vasp code {senderVaspCode} cannot be found during invitation processing");
                return;
            }

            _connections.TryGetValue(payload.ConnectionId, out var connection);
            if (connection != null)
            {
                bool isSameData = connection.CounterPartyVaspId == payload.SenderVaspId
                    && connection.OutboundTopic == payload.ReturnTopic;
                if (isSameData)
                {
                    _logger?.LogWarning(
                        $"Received invite for already existing connectionId {payload.ConnectionId} with the same data. Skipping.");

                    await AcknowledgeMessageAsync(
                        payload,
                        null,
                        payload.EcdhPk.DecompressPublicKey().ToHex(true));
                }
                else
                {
                    _logger?.LogWarning(
                        $"Received invite for already existing connectionId {payload.ConnectionId} with the different data:{Environment.NewLine}"
                        + $"SenderVaspId: {connection.CounterPartyVaspId} - {payload.SenderVaspId},{Environment.NewLine}"
                        + $"Topic: {connection.OutboundTopic} - {payload.ReturnTopic},{Environment.NewLine}");
                }
                return;
            }

            var topic = TopicGenerator.GenerateConnectionTopic();
            var sessionKey = ECDH_Key.GenerateKey();
            var sharedSecret = sessionKey.GenerateSharedSecretHex(payload.EcdhPk);
            var (filter, symKeyId) = await RegisterConnectionAsync(topic, sharedSecret);

            var newConnection = new Connection
            {
                Id = payload.ConnectionId,
                Filter = filter,
                InboundTopic = topic,
                OutboundTopic = payload.ReturnTopic,
                Status = ConnectionStatus.PartiallyActive,
                CounterPartyVaspId = payload.SenderVaspId,
                SymKeyId = symKeyId,
                SharedPrivateEncryptionKey = sharedSecret,
                PrivateKey = sessionKey.PrivateKey,
                CounterPartyPublicKey = payload.EcdhPk,
            };

            _connections[newConnection.Id] = newConnection;

            await AcknowledgeMessageAsync(
                payload,
                null,
                payload.EcdhPk.DecompressPublicKey().ToHex(true));

            var signingKey = await _vaspCodesService.GetSigningKeyAsync(senderVaspCode);
            await TriggerAsyncEvent(TransportMessageReceived,
                new TransportMessageEvent
                {
                    ConnectionId = payload.ConnectionId,
                    SenderVaspId = payload.SenderVaspId,
                    Instruction = payload.Instruction,
                    Payload = payload.OvMessage,
                    Timestamp = DateTime.UtcNow,
                    SigningKey = signingKey
                });
        }

        private async Task AcknowledgeMessageAsync(
            OpenVaspPayload payload,
            string symKey,
            string asymKey,
            string returnTopic = null)
        {
            var envelopeId = Guid.NewGuid().ToString("N");
            var ackPayload = new OpenVaspPayload(
                Instruction.Ack,
                _vaspId, // receiver becomes a sender
                payload.ConnectionId,
                envelopeId)
            {
                EnvelopeAck = payload.EnvelopeId
            };

            var ackEnvelope = new MessageEnvelope
            {
                Topic = returnTopic ?? payload.ReturnTopic,
                EncryptionKey = symKey ?? asymKey,
                EncryptionType = symKey != null ? EncryptionType.Symmetric : EncryptionType.Asymmetric,
            };

            await _outboundEnvelopeService.AcknowledgeAsync(ackEnvelope, ackPayload.ToString());

            _logger?.LogInformation(
                $"Sent ACK to {payload.Instruction} with topic {ackEnvelope.Topic} for connection {payload.ConnectionId} from {_vaspId} to {payload.SenderVaspId}");
        }

        private Task OnOutboundEnvelopeReachedMaxResends(OutboundEnvelopeReachedMaxResendsEvent arg)
        {
            _connections[arg.ConnectionId].Status = ConnectionStatus.Passive;
            
            return Task.CompletedTask;
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

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}