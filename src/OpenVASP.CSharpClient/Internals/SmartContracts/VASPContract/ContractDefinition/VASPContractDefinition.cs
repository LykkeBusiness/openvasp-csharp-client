using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace OpenVASP.CSharpClient.Internals.SmartContracts.ContractDefinition
{


    public partial class VASPContractDeployment : VASPContractDeploymentBase
    {
        public VASPContractDeployment() : base(BYTECODE) { }
        public VASPContractDeployment(string byteCode) : base(byteCode) { }
    }

    public class VASPContractDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public VASPContractDeploymentBase() : base(BYTECODE) { }
        public VASPContractDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class ChannelsFunction : ChannelsFunctionBase { }

    [Function("channels", "bytes4")]
    public class ChannelsFunctionBase : FunctionMessage
    {

    }

    public partial class MessageKeyFunction : MessageKeyFunctionBase { }

    [Function("messageKey", "bytes")]
    public class MessageKeyFunctionBase : FunctionMessage
    {

    }

    public partial class SigningKeyFunction : SigningKeyFunctionBase { }

    [Function("signingKey", "bytes")]
    public class SigningKeyFunctionBase : FunctionMessage
    {

    }

    public partial class TransportKeyFunction : TransportKeyFunctionBase { }

    [Function("transportKey", "bytes")]
    public class TransportKeyFunctionBase : FunctionMessage
    {

    }

    public partial class VaspCodeFunction : VaspCodeFunctionBase { }

    [Function("vaspCode", "bytes4")]
    public class VaspCodeFunctionBase : FunctionMessage
    {

    }

    public partial class ChannelsOutputDTO : ChannelsOutputDTOBase { }

    [FunctionOutput]
    public class ChannelsOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes4", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public partial class MessageKeyOutputDTO : MessageKeyOutputDTOBase { }

    [FunctionOutput]
    public class MessageKeyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class SigningKeyOutputDTO : SigningKeyOutputDTOBase { }

    [FunctionOutput]
    public class SigningKeyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class TransportKeyOutputDTO : TransportKeyOutputDTOBase { }

    [FunctionOutput]
    public class TransportKeyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class VaspCodeOutputDTO : VaspCodeOutputDTOBase { }

    [FunctionOutput]
    public class VaspCodeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes4", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }
}
