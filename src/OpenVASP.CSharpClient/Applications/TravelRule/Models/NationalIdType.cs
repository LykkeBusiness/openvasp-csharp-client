using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NationalIdType
    {
        [EnumMember(Value = "ARNU")]
        ForeignRegNumber,
        [EnumMember(Value = "CCPT")]
        PassportNumber,
        [EnumMember(Value = "RAID")]
        RegistrationAuthId,
        [EnumMember(Value = "DRLC")]
        DriverLicense,
        [EnumMember(Value = "FIIN")]
        ForeignInvestIdentityNum,
        [EnumMember(Value = "TXID")]
        TaxId,
        [EnumMember(Value = "SOCS")]
        SocialSecurityNum,
        [EnumMember(Value = "IDCN")]
        IdentityCardNum,
        [EnumMember(Value = "LEIX")]
        LegalEntityId,
        [EnumMember(Value = "MISC")]
        Unspecified
    }
}