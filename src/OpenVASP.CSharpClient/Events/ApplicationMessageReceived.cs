using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Internals.Messages;

namespace OpenVASP.CSharpClient.Events
{
    public class ApplicationMessageReceived
    {
        public MessageType Type { set; get; }
        public JObject Body { set; get; }
    }
}