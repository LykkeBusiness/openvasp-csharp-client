using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class Transfer
    {
        [JsonProperty("va")]
        public string VirtualAssetType { get; set; }

        [JsonProperty("ttype")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferType TransferType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}