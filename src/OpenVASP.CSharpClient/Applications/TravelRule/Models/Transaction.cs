using System;
using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class Transaction
    {
        [JsonProperty("txid")]
        public string TransactionHash { get; set; }

        [JsonProperty("txdate")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:ssZ")]
        public DateTime DateTime { get; set; }

        [JsonProperty("txsendadr")]
        public string SendingAddress { get; set; }
    }
}