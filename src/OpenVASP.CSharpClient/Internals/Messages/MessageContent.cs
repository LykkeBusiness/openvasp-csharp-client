using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenVASP.CSharpClient.Internals.Messages
{
    public class MessageContent
    {
        [JsonProperty("header")]
        public MessageHeader Header { get; set; }

        [JsonProperty("body")]
        public JObject RawBody { get; set; }
    }
}