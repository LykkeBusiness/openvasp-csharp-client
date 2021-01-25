using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenVASP.CSharpClient.Applications.TravelRule.Models;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Messages
{
    public class TransferReply
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("rcode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferReplyMessageCode Code { get; set; }

        [JsonProperty("transfer")]
        public TransferReply Transfer { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}