using System.Runtime.Serialization;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public enum TransferReplyMessageCode
    {
        [EnumMember(Value = "YES")]
        TransferAccepted = 1,
        [EnumMember(Value = "NO_INVALID")]
        TransferDeclinedRequestNotValid = 2,
        [EnumMember(Value = "NO_BENEFICIARY")]
        TransferDeclinedNoSuchBeneficiary = 3,
        [EnumMember(Value = "NO_VA")]
        TransferDeclinedVirtualAssetNotSupported = 4,
        [EnumMember(Value = "NO_AUTHORIZATION")]
        TransferDeclinedTransferNotAuthorized = 5,
        [EnumMember(Value = "NO_SERVICE")]
        TransferDeclinedTemporaryDisruptionOfService = 6,
    }
}