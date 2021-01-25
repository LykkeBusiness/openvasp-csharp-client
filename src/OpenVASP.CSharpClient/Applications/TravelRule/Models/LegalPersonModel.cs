using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class LegalPersonModel
    {
        [JsonProperty("name")]
        public LegalNameModel[] Names { get; set; }

        [JsonProperty("name_local")]
        public LegalNameModel[] LocalNames { get; set; }

        [JsonProperty("name_phonetic")]
        public LegalNameModel[] PhoneticNames { get; set; }

        [JsonProperty("ctry_reg")]
        public string RegistrationCountryIso2Code { get; set; }
    }
}