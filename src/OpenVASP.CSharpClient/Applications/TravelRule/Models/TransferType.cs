using System.Runtime.Serialization;

namespace OpenVASP.CSharpClient.Applications.TravelRule.Models
{
    public enum TransferType
    {
        [EnumMember(Value = "BCTX")]
        BlockchainTransfer = 1
    }
}