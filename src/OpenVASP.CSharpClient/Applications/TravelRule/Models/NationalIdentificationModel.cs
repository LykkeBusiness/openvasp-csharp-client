using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class NationalIdentificationModel
    {
        [JsonProperty("id_id")]
        public string NationalId { get; set; }

        [JsonProperty("id_type")]
        public NationalIdType NationalIdType { get; set; }

        [JsonProperty("id_ctry")]
        public string IssuerCountryIso2Code { get; set; }

        [JsonProperty("id_reg")]
        public string RegistrationAuthority { get; set; }
    }
}