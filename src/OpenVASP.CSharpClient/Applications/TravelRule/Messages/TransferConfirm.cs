using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenVASP.CSharpClient.Applications.TravelRule.Models;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Messages
{
    public class TransferConfirm
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("rcode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionConfirmCode Code { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}