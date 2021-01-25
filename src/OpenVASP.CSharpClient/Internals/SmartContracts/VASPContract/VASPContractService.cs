using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.ContractHandlers;
using System.Threading;
using OpenVASP.CSharpClient.Internals.SmartContracts.ContractDefinition;

namespace OpenVASP.CSharpClient.Internals.SmartContracts.VASPContract
{
    public partial class VASPContractService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, VASPContractDeployment vaspContractDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<VASPContractDeployment>().SendRequestAndWaitForReceiptAsync(vaspContractDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, VASPContractDeployment vaspContractDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<VASPContractDeployment>().SendRequestAsync(vaspContractDeployment);
        }

        public static async Task<VASPContractService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, VASPContractDeployment vaspContractDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, vaspContractDeployment, cancellationTokenSource);
            return new VASPContractService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public VASPContractService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<byte[]> ChannelsQueryAsync(ChannelsFunction channelsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ChannelsFunction, byte[]>(channelsFunction, blockParameter);
        }

        
        public Task<byte[]> ChannelsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ChannelsFunction, byte[]>(null, blockParameter);
        }

        public Task<byte[]> MessageKeyQueryAsync(MessageKeyFunction messageKeyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MessageKeyFunction, byte[]>(messageKeyFunction, blockParameter);
        }

        
        public Task<byte[]> MessageKeyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MessageKeyFunction, byte[]>(null, blockParameter);
        }

        public Task<byte[]> SigningKeyQueryAsync(SigningKeyFunction signingKeyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SigningKeyFunction, byte[]>(signingKeyFunction, blockParameter);
        }

        
        public Task<byte[]> SigningKeyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SigningKeyFunction, byte[]>(null, blockParameter);
        }

        public Task<byte[]> TransportKeyQueryAsync(TransportKeyFunction transportKeyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TransportKeyFunction, byte[]>(transportKeyFunction, blockParameter);
        }

        
        public Task<byte[]> TransportKeyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TransportKeyFunction, byte[]>(null, blockParameter);
        }

        public Task<byte[]> VaspCodeQueryAsync(VaspCodeFunction vaspCodeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<VaspCodeFunction, byte[]>(vaspCodeFunction, blockParameter);
        }

        
        public Task<byte[]> VaspCodeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<VaspCodeFunction, byte[]>(null, blockParameter);
        }
    }
}
