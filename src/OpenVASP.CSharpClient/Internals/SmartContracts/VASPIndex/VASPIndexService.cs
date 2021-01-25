using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.ContractHandlers;
using System.Threading;
using OpenVASP.CSharpClient.Internals.SmartContracts.VASPIndex.ContractDefinition;

namespace OpenVASP.CSharpClient.Internals.SmartContracts.VASPIndex
{
    public partial class VASPIndexService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, VASPIndexDeployment vaspIndexDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<VASPIndexDeployment>().SendRequestAndWaitForReceiptAsync(vaspIndexDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, VASPIndexDeployment vaspIndexDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<VASPIndexDeployment>().SendRequestAsync(vaspIndexDeployment);
        }

        public static async Task<VASPIndexService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, VASPIndexDeployment vaspIndexDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, vaspIndexDeployment, cancellationTokenSource);
            return new VASPIndexService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public VASPIndexService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<string> GetVaspAddressByCodeQueryAsync(GetVASPAddressByCodeFunction getVaspAddressByCodeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetVASPAddressByCodeFunction, string>(getVaspAddressByCodeFunction, blockParameter);
        }

        
        public Task<string> GetVaspAddressByCodeQueryAsync(byte[] vaspCode, BlockParameter blockParameter = null)
        {
            var getVaspAddressByCodeFunction = new GetVASPAddressByCodeFunction();
                getVaspAddressByCodeFunction.VaspCode = vaspCode;
            
            return ContractHandler.QueryAsync<GetVASPAddressByCodeFunction, string>(getVaspAddressByCodeFunction, blockParameter);
        }

        public Task<byte[]> GetVaspCodeByAddressQueryAsync(GetVASPCodeByAddressFunction getVaspCodeByAddressFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetVASPCodeByAddressFunction, byte[]>(getVaspCodeByAddressFunction, blockParameter);
        }

        
        public Task<byte[]> GetVaspCodeByAddressQueryAsync(string vaspAddress, BlockParameter blockParameter = null)
        {
            var getVaspCodeByAddressFunction = new GetVASPCodeByAddressFunction();
                getVaspCodeByAddressFunction.VaspAddress = vaspAddress;
            
            return ContractHandler.QueryAsync<GetVASPCodeByAddressFunction, byte[]>(getVaspCodeByAddressFunction, blockParameter);
        }
    }
}
