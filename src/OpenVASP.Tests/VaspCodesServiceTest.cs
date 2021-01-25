using System.Threading.Tasks;
using Nethereum.Web3;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Services;
using Xunit;

namespace OpenVASP.Tests
{
    public class VaspCodesServiceTest
    {
        private readonly IVaspCodesService _vaspCodesService;
        private const string _vaspCode1 = "00000001";
        private const string _vaspCode2 = "00000002";

        public VaspCodesServiceTest()
        {
            var ethereumRpcUrl = "https://ropsten.infura.io/v3/fb49e892176d413d85f993d0352a0971";
            var indexAddress = "0x4988a15D3CA2AEBd2Dfd02629E759492E6e29BfE";
            
            _vaspCodesService = new VaspCodesService(new Web3(ethereumRpcUrl), indexAddress);
        }

        [Theory]
        [InlineData("02b2fe0b7fdefef41e8d9d30ebf256d8975f281cfe1388a8d6cfe38dcdf9e13402", _vaspCode1)]
        [InlineData("020455fc27e487828602eada0b3b6c2a14c8c8deeb7f4a7e84eeebc8696ba2b6bf", _vaspCode2)]
        public async Task MessageKeysAsync(string value, string vaspCode)
        {
            Assert.Equal(value, await _vaspCodesService.GetMessageKeyAsync(vaspCode));
        }

        [Theory]
        [InlineData("0259d614774a057f68e62b85d46c921286cab02a0dc43de89a72c13e9c86d467c7", _vaspCode1)]
        [InlineData("026e1c88431121b2019beee19e61d42036c6e0d182c7e47e08370382e6aa9f82bd", _vaspCode2)]
        public async Task SigningKeysAsync(string value, string vaspCode)
        {
            Assert.Equal(value, await _vaspCodesService.GetSigningKeyAsync(vaspCode));
        }

        [Theory]
        [InlineData("0286f2b8ddcb89952c29218ec19f675010e0834171c7ab6cc5efbf9e1d1e8a90dc", _vaspCode1)]
        [InlineData("02199c544f3f8b3177c5b9eee787f6d947a1b01554c75dfe3299ac44118aae7cbc", _vaspCode2)]
        public async Task TransportKeysAsync(string value, string vaspCode)
        {
            Assert.Equal(value, await _vaspCodesService.GetTransportKeyAsync(vaspCode));
        }
    }
}