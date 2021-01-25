using Newtonsoft.Json;
using OpenVASP.CSharpClient.Applications.TravelRule.Models;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Messages
{
    public class TransferRequest
    {
        [JsonProperty("originator")]
        public Originator Originator { get; set; }

        [JsonProperty("beneficiary")]
        public Beneficiary Beneficiary { get; set; }

        [JsonProperty("transfer")]
        public Transfer Transfer { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}