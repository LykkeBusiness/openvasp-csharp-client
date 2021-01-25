using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Messages.Session;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface ISessionsService : IDisposable
    {
        event Func<BeneficiarySessionCreatedEvent, Task> BeneficiarySessionCreated;
        event Func<OriginatorSessionApprovedEvent, Task> OriginatorSessionApproved;
        event Func<OriginatorSessionDeclinedEvent, Task> OriginatorSessionDeclined;
        event Func<SessionAbortedEvent, Task> SessionAborted;
        event Func<SessionTerminatedEvent, Task> SessionTerminated;
        event Func<ApplicationMessageReceivedEvent, Task> ApplicationMessageReceived;
        
        Task<string> CreateSessionAndSendSessionRequestAsync(string counterPartyVaspId);
        Task SessionReplyAsync(string sessionId, SessionReplyMessageCode code);
        Task SessionAbortAsync(string sessionId, SessionAbortCause cause);
        Task TerminateSessionAsync(string sessionId);
        Task SendMessageAsync(string sessionId, MessageType type, JObject body);
    }
}