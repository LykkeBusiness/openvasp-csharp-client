using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Events;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Messages.Session;
using OpenVASP.CSharpClient.Internals.Services;

namespace OpenVASP.CSharpClient
{
    public class VaspClient : IVaspClient, IDisposable
    {
        private readonly ISessionsService _sessionsService;

        public event Func<SessionAborted, Task> SessionAbortedEvent;
        public event Func<SessionTerminated, Task> SessionTerminatedEvent;
        public event Func<BeneficiarySessionCreated, Task> BeneficiarySessionCreatedEvent;
        public event Func<OriginatorSessionDeclined, Task> OriginatorSessionDeclinedEvent;
        public event Func<OriginatorSessionApproved, Task> OriginatorSessionApprovedEvent;
        public event Func<ApplicationMessageReceived, Task> ApplicationMessageReceivedEvent;

        private VaspClient(ISessionsService sessionsService)
        {
            _sessionsService = sessionsService;
            
            _sessionsService.BeneficiarySessionCreated += OnBeneficiarySessionCreated;
            _sessionsService.OriginatorSessionApproved += OnOriginatorSessionApproved;
            _sessionsService.OriginatorSessionDeclined += OnOriginatorSessionDeclined;
            _sessionsService.ApplicationMessageReceived += OnApplicationMessageReceived;
            _sessionsService.SessionAborted += OnSessionAborted;
            _sessionsService.SessionTerminated += OnSessionTerminated;
        }

        public static VaspClient Create(VaspClientSettings settings)
        {
            var signService = new MessageSignService(settings.PrivateSigningKey);
            
            var messageFormatterService = new MessageFormatterService(signService, settings.VaspId);
            
            var whisperService = new WhisperService(new Web3(settings.WhisperRpc), settings.LoggerFactory);

            var outboundEnvelopeService = new OutboundEnvelopeService(
                whisperService, settings.EnvelopeExpirySeconds,
                settings.EnvelopeMaxRetries, settings.LoggerFactory);
            
            var vaspCodesService = new VaspCodesService(new Web3(settings.EthereumRpc), settings.IndexSmartContractAddress);

            var transportService = new TransportClient(settings.VaspId, settings.PrivateTransportKey, whisperService,
                vaspCodesService, outboundEnvelopeService, settings.LoggerFactory);

            var sessionsService = new SessionsService(messageFormatterService, vaspCodesService, transportService,
                settings.PrivateMessageKey, settings.LoggerFactory);
            
            var vaspClient = new VaspClient(sessionsService);

            return vaspClient;
        }
        
        public async Task<string> CreateSessionAsync(string vaspId)
        {
            return await _sessionsService.CreateSessionAndSendSessionRequestAsync(vaspId);
        }

        public async Task SessionReplyAsync(string sessionId, bool approve)
        {
            if (approve)
            {
                await _sessionsService.SessionReplyAsync(sessionId, SessionReplyMessageCode.SessionAccepted);
            }
            else
            {
                await _sessionsService.SessionReplyAsync(sessionId, SessionReplyMessageCode.SessionDeclinedOriginatorVaspDeclined);
            }
        }

        public async Task SendApplicationMessageAsync(string sessionId, MessageType type, JObject body)
        {
            await _sessionsService.SendMessageAsync(sessionId, type, body);
        }

        public async Task SessionTerminateAsync(string sessionId)
        {
            await _sessionsService.TerminateSessionAsync(sessionId);
        }

        private async Task OnSessionTerminated(SessionTerminatedEvent arg)
        {
            await TriggerAsyncEvent(SessionTerminatedEvent, new SessionTerminated
            {
                SessionId = arg.SessionId
            });
        }

        private async Task OnSessionAborted(SessionAbortedEvent arg)
        {
            await TriggerAsyncEvent(SessionAbortedEvent, new SessionAborted
            {
                SessionId = arg.SessionId
            });
        }

        private async Task OnApplicationMessageReceived(ApplicationMessageReceivedEvent arg)
        {
            await TriggerAsyncEvent(ApplicationMessageReceivedEvent, new ApplicationMessageReceived
            {
                Type = arg.Type,
                Body = arg.Payload
            });
        }

        private async Task OnOriginatorSessionDeclined(OriginatorSessionDeclinedEvent arg)
        {
            await TriggerAsyncEvent(OriginatorSessionDeclinedEvent, new OriginatorSessionDeclined
            {
                SessionId = arg.SessionId
            });
        }

        private async Task OnOriginatorSessionApproved(OriginatorSessionApprovedEvent arg)
        {
            await TriggerAsyncEvent(OriginatorSessionApprovedEvent, new OriginatorSessionApproved
            {
                SessionId = arg.SessionId
            });
        }

        private async Task OnBeneficiarySessionCreated(BeneficiarySessionCreatedEvent arg)
        {
            await TriggerAsyncEvent(BeneficiarySessionCreatedEvent, new BeneficiarySessionCreated
            {
                SessionId = arg.SessionId,
                CounterPartyVaspId = arg.CounterPartyVaspId
            });
        }

        public void Dispose()
        {
            _sessionsService.Dispose();
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
    
    public class VaspClientSettings
    {
        public string VaspId { set; get; }
        public string WhisperRpc { set; get; }
        public string EthereumRpc { set; get; }
        public string PrivateSigningKey { set; get; }
        public string PrivateTransportKey { set; get; }
        public string PrivateMessageKey { set; get; }
        public string IndexSmartContractAddress { set; get; }
        public int EnvelopeMaxRetries { set; get; }
        public int EnvelopeExpirySeconds { set; get; }
        public ILoggerFactory LoggerFactory { set; get; }
    }
}