using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Internals.Messages;

namespace OpenVASP.CSharpClient.Internals.Events
{
    public class ApplicationMessageReceivedEvent : SessionEventBase
    {
        public MessageType Type { set; get; }
        public JObject Payload { set; get; }
    }
}