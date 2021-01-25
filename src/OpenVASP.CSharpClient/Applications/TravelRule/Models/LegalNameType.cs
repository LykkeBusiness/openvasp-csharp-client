using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LegalNameType
    {
        [EnumMember(Value = "LEGL")]
        LegalName,
        [EnumMember(Value = "SHRT")]
        ShortName,
        [EnumMember(Value = "TRAD")]
        TradingName
    }
}