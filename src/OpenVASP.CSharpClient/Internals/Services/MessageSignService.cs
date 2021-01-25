using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using OpenVASP.CSharpClient.Internals.Interfaces;

namespace OpenVASP.CSharpClient.Internals.Services
{
    public class MessageSignService : IMessageSignService
    {
        private readonly EthereumMessageSigner _signer;
        private readonly string _privateSigningKey;

        public MessageSignService(string privateSigningKey)
        {
            _privateSigningKey = privateSigningKey;
            _signer = new EthereumMessageSigner();
        }

        public async Task<string> SignPayloadAsync(string payload)
        {
            var sign = _signer.EncodeUTF8AndSign(payload, new EthECKey(_privateSigningKey));
            return sign.RemoveHexPrefix();
        }

        public bool VerifySign(string payload, string sign, string pubKey)
        {
            var expectedSigner = new EthECKey(pubKey.HexToByteArray(), false);
            var publicAddress = expectedSigner.GetPublicAddress();
            var signerAddress = _signer.EncodeUTF8AndEcRecover(payload, sign);

            return publicAddress.Equals(signerAddress, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}