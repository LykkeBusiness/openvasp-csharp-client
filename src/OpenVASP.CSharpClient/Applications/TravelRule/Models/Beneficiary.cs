using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class Beneficiary
    {
        [JsonProperty("natural")]
        public NaturalPersonModel Natural { get; set; }

        [JsonProperty("legal")]
        public LegalPersonModel Legal { get; set; }

        [JsonProperty("address")]
        public AddressModel Address { get; set; }

        [JsonProperty("id")]
        public NationalIdentificationModel[] NationalIds { get; set; }

        [JsonProperty("cid")]
        public CustomerIdentificationModel[] CustomerIds { get; set; }
    }
}