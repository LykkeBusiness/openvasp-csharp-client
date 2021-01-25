using Newtonsoft.Json;
using OpenVASP.CSharpClient.Applications.TravelRule.Models;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Messages
{
    public class TransferDispatch
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("tx")]
        public Transaction Transfer { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}