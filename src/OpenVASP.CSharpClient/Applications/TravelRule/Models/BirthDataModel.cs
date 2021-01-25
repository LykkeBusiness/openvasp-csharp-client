using System;
using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class BirthDataModel
    {
        [JsonProperty("birthdate")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddThh:mm:ssZ")]
        public DateTime BirthDate { get; set; }

        [JsonProperty("birthplace")]
        public string BirthPlace { get; set; }
    }
}