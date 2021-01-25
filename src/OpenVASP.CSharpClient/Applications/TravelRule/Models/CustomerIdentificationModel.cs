using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class CustomerIdentificationModel
    {
        [JsonIgnore]
        public string Id { get; set; }
        
        [JsonProperty("vaan")]
        public string Vaan { get; set; }

        [JsonProperty("cid_id")]
        public string GenericId { get; set; }
    }
}