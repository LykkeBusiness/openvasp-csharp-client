using System.Runtime.Serialization;

namespace OpenVASP.CSharpClient.Internals.Messages
{
    public enum MessageType
    {
        [EnumMember(Value = "100")]
        SessionRequest = 100,

        [EnumMember(Value = "200")]
        SessionReply = 200,

        [EnumMember(Value = "300")]
        Termination = 300,

        [EnumMember(Value = "400")]
        Abort = 400,

        [EnumMember(Value = "TFR.100")]
        TransferRequest = 1100,

        [EnumMember(Value = "TFR.200")]
        TransferReply = 1200,

        [EnumMember(Value = "TFR.300")]
        TransferDispatch = 1300,

        [EnumMember(Value = "TFR.400")]
        TransferConfirmation = 1400,
    }
}