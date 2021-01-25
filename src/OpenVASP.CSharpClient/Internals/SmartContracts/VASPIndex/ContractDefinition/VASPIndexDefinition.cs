using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace OpenVASP.CSharpClient.Internals.SmartContracts.VASPIndex.ContractDefinition
{
    public partial class VASPIndexDeployment : VASPIndexDeploymentBase
    {
        public VASPIndexDeployment() : base(BYTECODE) { }
        public VASPIndexDeployment(string byteCode) : base(byteCode) { }
    }

    public class VASPIndexDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public VASPIndexDeploymentBase() : base(BYTECODE) { }
        public VASPIndexDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class GetVASPAddressByCodeFunction : GetVASPAddressByCodeFunctionBase { }

    [Function("getVASPAddressByCode", "address")]
    public class GetVASPAddressByCodeFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "vaspCode", 1)]
        public virtual byte[] VaspCode { get; set; }
    }

    public partial class GetVASPCodeByAddressFunction : GetVASPCodeByAddressFunctionBase { }

    [Function("getVASPCodeByAddress", "bytes4")]
    public class GetVASPCodeByAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "vaspAddress", 1)]
        public virtual string VaspAddress { get; set; }
    }

    public partial class GetVASPAddressByCodeOutputDTO : GetVASPAddressByCodeOutputDTOBase { }

    [FunctionOutput]
    public class GetVASPAddressByCodeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class GetVASPCodeByAddressOutputDTO : GetVASPCodeByAddressOutputDTOBase { }

    [FunctionOutput]
    public class GetVASPCodeByAddressOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes4", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }
}
