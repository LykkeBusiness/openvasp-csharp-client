using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class LegalNameModel
    {
        [JsonProperty("leg_name")]
        public string Name { get; set; }

        [JsonProperty("leg_nametype")]
        public LegalNameType NameType { get; set; }
    }
}