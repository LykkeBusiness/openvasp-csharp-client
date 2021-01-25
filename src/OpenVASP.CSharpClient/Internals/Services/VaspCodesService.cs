using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.SmartContracts.VASPContract;
using OpenVASP.CSharpClient.Internals.SmartContracts.VASPIndex;

namespace OpenVASP.CSharpClient.Internals.Services
{
    public class VaspCodesService : IVaspCodesService
    {
        private readonly IWeb3 _web3;
        private readonly string _vaspIndexAddress;

        public VaspCodesService(IWeb3 web3, string vaspIndexAddress)
        {
            _vaspIndexAddress = vaspIndexAddress;
            _web3 = web3;
        }
        
        public async Task<string> GetMessageKeyAsync(string vaspCode)
        {
            return await FetchMessageKeyAsync(await FetchVaspContractAddressAsync(vaspCode));
        }
        
        public async Task<string> GetSigningKeyAsync(string vaspCode)
        {
            return await FetchSigningKeyAsync(await FetchVaspContractAddressAsync(vaspCode));
        }
        
        public async Task<string> GetTransportKeyAsync(string vaspCode)
        {
            return await FetchTransportKeyAsync(await FetchVaspContractAddressAsync(vaspCode));
        }
        
        private async Task<string> FetchVaspContractAddressAsync(
            string vaspCode)
        {
            return await new VASPIndexService(_web3, _vaspIndexAddress).GetVaspAddressByCodeQueryAsync(vaspCode.HexToByteArray());
        }
        
        private async Task<string> FetchMessageKeyAsync(string vaspSmartContractAddress)
        {
            var vaspContract = new VASPContractService(_web3, vaspSmartContractAddress);
            var transportKeyBytes = await vaspContract.MessageKeyQueryAsync();
            return transportKeyBytes.ToHex();
        }
        
        private async Task<string> FetchTransportKeyAsync(string vaspSmartContractAddress)
        {
            var vaspContract = new VASPContractService(_web3, vaspSmartContractAddress);
            var transportKeyBytes = await vaspContract.TransportKeyQueryAsync();
            return transportKeyBytes.ToHex();
        }

        private async Task<string> FetchSigningKeyAsync(string vaspSmartContractAddress)
        {
            var vaspContract = new VASPContractService(_web3, vaspSmartContractAddress);
            var signingKeyBytes = await vaspContract.SigningKeyQueryAsync();
            return signingKeyBytes.ToHex();
        }
    }
}