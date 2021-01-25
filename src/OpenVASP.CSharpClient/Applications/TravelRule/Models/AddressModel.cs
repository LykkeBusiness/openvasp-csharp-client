using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public class AddressModel
    {
        [JsonProperty("adr_type")]
        public AddressType AddressType { get; set; }

        [JsonProperty("dep")]
        public string Department { get; set; }

        [JsonProperty("dep_sub")]
        public string SubDepartment { get; set; }

        [JsonProperty("street")]
        public string StreetName { get; set; }

        [JsonProperty("bldg_no")]
        public string BuildingNumber { get; set; }

        [JsonProperty("bldg")]
        public string BuildingName { get; set; }

        [JsonProperty("floor")]
        public string Floor { get; set; }

        [JsonProperty("box")]
        public string PostOfficeBox { get; set; }

        [JsonProperty("postcode")]
        public string PostCode { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("town")]
        public string Town { get; set; }

        [JsonProperty("location")]
        public string TownLocation { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("ctry")]
        public string CountryIso2Code { get; set; }

        [JsonProperty("country_sub")]
        public string CountrySubdivision { get; set; }

        [JsonProperty("adr_line")]
        public string[] AddressLines { get; set; }
    }
}