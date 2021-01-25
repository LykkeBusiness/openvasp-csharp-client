using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class NaturalNameModel
    {
        [JsonProperty("name_primary")]
        public string PrimaryName { get; set; }

        [JsonProperty("name_secondary")]
        public string SecondaryName { get; set; }

        [JsonProperty("name_type")]
        public NaturalNameType NameType { get; set; }
    }
}