using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class NaturalPersonModel
    {
        [JsonProperty("name")]
        public NaturalNameModel[] Names { get; set; }

        [JsonProperty("name_local")]
        public NaturalNameModel[] LocalNames { get; set; }

        [JsonProperty("name_phonetic")]
        public NaturalNameModel[] PhoneticNames { get; set; }

        [JsonProperty("birth")]
        public BirthDataModel BirthData { get; set; }

        [JsonProperty("ctry_residence")]
        public string ResidenceCountryIso2Code { get; set; }
    }
}