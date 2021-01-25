using System.Runtime.Serialization;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public enum TransactionConfirmCode
    {
        [EnumMember(Value = "YES")]
        TransferConfirmed,
        [EnumMember(Value = "NO_RECEIPT")]
        AssetNotReceived,
        [EnumMember(Value = "NO_INVALID")]
        DispatchInvalid,
        [EnumMember(Value = "NO_AMOUNT")]
        AmountInvalid,
        [EnumMember(Value = "NO_VA")]
        WrongAsset,
        [EnumMember(Value = "NO_TXDATA")]
        NoTransactionId
    }
}