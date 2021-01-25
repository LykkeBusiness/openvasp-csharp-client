using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Events;
using OpenVASP.CSharpClient.Internals.Messages;

namespace OpenVASP.CSharpClient
{
    public interface IVaspClient
    {
        event Func<SessionTerminated, Task> SessionTerminatedEvent;
        event Func<SessionAborted, Task> SessionAbortedEvent;
        event Func<BeneficiarySessionCreated, Task> BeneficiarySessionCreatedEvent;
        event Func<OriginatorSessionDeclined, Task> OriginatorSessionDeclinedEvent;
        event Func<OriginatorSessionApproved, Task> OriginatorSessionApprovedEvent;
        event Func<ApplicationMessageReceived, Task> ApplicationMessageReceivedEvent;

        Task<string> CreateSessionAsync(string vaspId);
        Task SessionReplyAsync(string sessionId, bool approve);
        Task SendApplicationMessageAsync(string sessionId, MessageType type, JObject body);
    }
}