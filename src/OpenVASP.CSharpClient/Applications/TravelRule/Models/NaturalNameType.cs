using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NaturalNameType
    {
        [EnumMember(Value = "ALIA")]
        AliasName,
        [EnumMember(Value = "BIRT")]
        NameAtBirth,
        [EnumMember(Value = "MAID")]
        MaidenName,
        [EnumMember(Value = "LEGL")]
        LegalName,
        [EnumMember(Value = "MISC")]
        Unspecified
    }
}