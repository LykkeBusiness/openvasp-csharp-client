using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Internals.Cryptography;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Messages.Session;
using OpenVASP.CSharpClient.Internals.Models;
using Org.BouncyCastle.Crypto;

namespace OpenVASP.CSharpClient.Internals.Services
{
    internal class SessionsService : ISessionsService
    {
        private readonly IMessageFormatterService _messageFormatterService;
        private readonly IVaspCodesService _vaspCodesService;
        private readonly ITransportService _transportService;
        private readonly ConcurrentDictionary<string, Session> _sessions;
        private readonly string _privateMessageKey;
        private readonly ILogger<SessionsService> _logger;

        public event Func<BeneficiarySessionCreatedEvent, Task> BeneficiarySessionCreated;
        public event Func<OriginatorSessionApprovedEvent, Task> OriginatorSessionApproved;
        public event Func<OriginatorSessionDeclinedEvent, Task> OriginatorSessionDeclined;
        public event Func<SessionAbortedEvent, Task> SessionAborted;
        public event Func<SessionTerminatedEvent, Task> SessionTerminated;
        public event Func<ApplicationMessageReceivedEvent, Task> ApplicationMessageReceived;

        public SessionsService(
            IMessageFormatterService messageFormatterService,
            IVaspCodesService vaspCodesService,
            ITransportService transportService,
            string privateMessageKey,
            ILoggerFactory loggerFactory)
        {
            _messageFormatterService = messageFormatterService;
            _vaspCodesService = vaspCodesService;
            _transportService = transportService;
            _sessions = new ConcurrentDictionary<string, Session>();
            _privateMessageKey = privateMessageKey;
            _logger = loggerFactory?.CreateLogger<SessionsService>();

            _transportService.TransportMessageReceived += TransportClientOnTransportMessageReceived;
        }

        public async Task<string> CreateSessionAndSendSessionRequestAsync(string counterPartyVaspId)
        {
            var session = new Session
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = SessionType.Originator,
                State = SessionState.Created,
                CounterPartyVaspId = counterPartyVaspId,
                CreationDateTime = DateTime.UtcNow,
                EcdhPrivateKey = ECDH_Key.GenerateKey().PrivateKey,
            };

            _sessions[session.Id] = session;

            var whisperConnection = await _transportService.CreateConnectionAsync(session.CounterPartyVaspId);

            session.ConnectionId = whisperConnection;

            var messageKey = await _vaspCodesService.GetMessageKeyAsync(session.CounterPartyVaspId.Substring(4));
            session.TempAesMessageKey = ECDH_Key.ImportKey(_privateMessageKey).GenerateSharedSecretHex(messageKey);

            await SendMessageAsync(
                session,
                MessageType.SessionRequest,
                Instruction.Invite,
                ECDH_Key.ImportKey(session.EcdhPrivateKey).PublicKey,
                new SessionRequest());

            return session.Id;
        }

        public async Task SessionReplyAsync(
            string sessionId,
            SessionReplyMessageCode code)
        {
            var session = _sessions[sessionId];

            await SendMessageAsync(
                session,
                MessageType.SessionReply,
                code == SessionReplyMessageCode.SessionAccepted ? Instruction.Accept : Instruction.Deny,
                ECDH_Key.ImportKey(session.EcdhPrivateKey).PublicKey,
                new SessionReply
                {
                    Code = ((int) code).ToString()
                });
        }

        public async Task SessionAbortAsync(
            string sessionId,
            SessionAbortCause cause)
        {
            var session = _sessions[sessionId];

            await SendMessageAsync(
                session,
                MessageType.Abort,
                Instruction.Close,
                null,
                new SessionAbort
                {
                    Code = ((int) cause).ToString()
                });
        }

        public async Task TerminateSessionAsync(string sessionId)
        {
            var session = _sessions[sessionId];

            await SendMessageAsync(
                session,
                MessageType.Termination,
                Instruction.Close,
                null,
                new SessionTermination());
        }

        public async Task SendMessageAsync(string sessionId, MessageType type, JObject body)
        {
            var session = _sessions[sessionId];

            await SendMessageAsync(
                session,
                type,
                Instruction.Update,
                null,
                body);
        }

        private async Task TransportClientOnTransportMessageReceived(TransportMessageEvent evt)
        {
                MessageContent messageContent;
                string messagePlaintext;
                string sig;

                var session = _sessions.Values.SingleOrDefault(x => x.ConnectionId == evt.ConnectionId);

                if (session == null)
                {
                    var messageKey = await _vaspCodesService.GetMessageKeyAsync(evt.SenderVaspId.Substring(4));
                    var aesMessageKey = ECDH_Key.ImportKey(_privateMessageKey).GenerateSharedSecretHex(messageKey);
                    (messageContent, messagePlaintext, sig) =
                        _messageFormatterService.Deserialize(evt.Payload, aesMessageKey, evt.SigningKey);
                }
                else if (string.IsNullOrWhiteSpace(session.EstablishedAesMessageKey))
                {
                    (messageContent, messagePlaintext, sig) =
                        _messageFormatterService.Deserialize(evt.Payload, session.TempAesMessageKey, evt.SigningKey);
                }
                else
                {
                    try
                    {
                        (messageContent, messagePlaintext, sig) =
                            _messageFormatterService.Deserialize(evt.Payload, session.EstablishedAesMessageKey,
                                evt.SigningKey);
                    }
                    catch (InvalidCipherTextException)
                    {
                        (messageContent, messagePlaintext, sig) =
                            _messageFormatterService.Deserialize(evt.Payload, session.TempAesMessageKey,
                                evt.SigningKey);
                    }
                }

                switch (evt.Instruction)
                {
                    case Instruction.Invite:
                        await HandleSessionRequestAsync(
                            evt.ConnectionId,
                            messageContent.Header.SessionId,
                            evt.SenderVaspId,
                            messageContent.Header.EcdhPk);
                        break;
                    case Instruction.Accept:
                    case Instruction.Deny:
                        var messageCode = messageContent.RawBody.ToObject<SessionReply>().Code;
                        await HandleSessionReplyAsync(
                            messageContent.Header.SessionId,
                            messageCode,
                            messageContent.Header.EcdhPk);
                        break;
                    case Instruction.Close:
                        switch (messageContent.Header.MessageType)
                        {
                            case MessageType.Abort:
                                await HandleSessionAbortAsync(
                                    messageContent.Header.SessionId,
                                    messageContent.RawBody.ToObject<SessionAbort>().Code);
                                break;
                            case MessageType.Termination:
                                await HandleTerminationAsync(messageContent.Header.SessionId);
                                break;
                        }

                        break;
                    case Instruction.Update:
                        await TriggerAsyncEvent(ApplicationMessageReceived, new ApplicationMessageReceivedEvent
                        {
                            Type = messageContent.Header.MessageType,
                            SessionId = session.Id,
                            Payload = messageContent.RawBody
                        });
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Instruction type {Enum.GetName(typeof(Instruction), evt.Instruction)} is not supported");
                }
        }

        private async Task HandleSessionRequestAsync(
            string connectionId,
            string sessionId,
            string senderVaspId,
            string ecdhPkA)
        {
            var key = ECDH_Key.GenerateKey();

            var session = new Session
            {
                Id = sessionId,
                Type = SessionType.Beneficiary,
                State = SessionState.Invited,
                CounterPartyVaspId = senderVaspId,
                ConnectionId = connectionId,
                CreationDateTime = DateTime.UtcNow,
                EcdhPrivateKey = key.PrivateKey,
                TempAesMessageKey = ECDH_Key.ImportKey(_privateMessageKey)
                    .GenerateSharedSecretHex(await _vaspCodesService.GetMessageKeyAsync(senderVaspId.Substring(4)))
            };

            if (!string.IsNullOrWhiteSpace(ecdhPkA))
                session.EstablishedAesMessageKey = key.GenerateSharedSecretHex(ecdhPkA);

            _sessions[sessionId] = session;

            await TriggerAsyncEvent(BeneficiarySessionCreated, new BeneficiarySessionCreatedEvent
            {
                SessionId = sessionId,
                CounterPartyVaspId = session.CounterPartyVaspId
            });
        }

        public async Task HandleSessionReplyAsync(string sessionId, string messageCode, string ecdhPkB)
        {
            var session = _sessions[sessionId];

            if (session.State != SessionState.Initiated)
            {
                _logger?.LogWarning(
                    $"Can't process reply for session {sessionId} that is in {session.State} state. Skipping.");
                return;
            }

            var sessionReplyCode = (SessionReplyMessageCode) Enum.Parse(typeof(SessionReplyMessageCode), messageCode);
            session.State = sessionReplyCode == SessionReplyMessageCode.SessionAccepted
                ? SessionState.Open
                : SessionState.Declined;

            if (!string.IsNullOrWhiteSpace(ecdhPkB))
                session.EstablishedAesMessageKey =
                    ECDH_Key.ImportKey(session.EcdhPrivateKey).GenerateSharedSecretHex(ecdhPkB);

            if (session.State == SessionState.Open)
            {
                await TriggerAsyncEvent(OriginatorSessionApproved, new OriginatorSessionApprovedEvent
                {
                    SessionId = sessionId
                });
            }
            else
            {
                await TriggerAsyncEvent(OriginatorSessionDeclined, new OriginatorSessionDeclinedEvent
                {
                    SessionId = sessionId
                });
            }
        }

        public async Task HandleSessionAbortAsync(string sessionId, string abortCause)
        {
            var session = _sessions[sessionId];

            session.State = SessionState.Aborted;

            await TriggerAsyncEvent(SessionAborted, new SessionAbortedEvent
            {
                SessionId = sessionId
            });
        }

        public async Task HandleTerminationAsync(string sessionId)
        {
            var session = _sessions[sessionId];

            session.State = SessionState.Closed;

            await TriggerAsyncEvent(SessionTerminated, new SessionTerminatedEvent
            {
                SessionId = sessionId
            });
        }

        private async Task SendMessageAsync(
            Session session,
            MessageType messageType,
            Instruction instruction,
            string ecdhPk,
            object messageBody)
        {
            var aesKey = session.State == SessionState.Invited
                ? session.TempAesMessageKey
                : session.EstablishedAesMessageKey ?? session.TempAesMessageKey;

            session.State = messageType switch
            {
                MessageType.SessionRequest => SessionState.Initiated,
                MessageType.SessionReply => instruction == Instruction.Accept
                    ? SessionState.Open
                    : SessionState.Declined,
                MessageType.Termination => SessionState.Closed,
                MessageType.Abort => SessionState.Aborted,
                _ => session.State
            };

            var (payload, messagePlaintext) = await _messageFormatterService.GetPayloadAsync(
                session.CounterPartyVaspId,
                session.Id,
                messageType,
                ecdhPk,
                JObject.FromObject(messageBody),
                aesKey);

            await _transportService.SendAsync(
                session.ConnectionId,
                payload,
                instruction,
                session.CounterPartyVaspId);
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
            _transportService.Dispose();
        }
    }
}