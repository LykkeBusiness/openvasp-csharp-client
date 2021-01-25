using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Utils;

namespace OpenVASP.CSharpClient.Internals.Services
{
    public class MessageFormatterService : IMessageFormatterService
    {
        private readonly IMessageSignService _signService;
        private readonly string _vaspId;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public MessageFormatterService(
            IMessageSignService signService,
            string vaspId)
        {
            _signService = signService;
            _vaspId = vaspId;
        }

        public async Task<(string, string)> GetPayloadAsync(
            string targetVaspId,
            string sessionId,
            MessageType messageType,
            string ecdhPk,
            JObject messageBody,
            string aesKeyHex)
        {
            var content = new MessageContent
            {
                Header = new MessageHeader(
                    _vaspId,
                    targetVaspId,
                    Guid.NewGuid().ToString("N"),
                    sessionId,
                    messageType,
                    ecdhPk),
                RawBody = messageBody,
            };
            
            var contentJson = JsonConvert.SerializeObject(content, settings: JsonSerializerSettings);

            var sig = await _signService.SignPayloadAsync(contentJson);

            var encryptionBytes = aesKeyHex.EnsureHexPrefix().HexToByteArray();
            var encrypted = sig.HexToByteArray().Concat(contentJson.ToHexUTF8().HexToByteArray()).ToArray().EncryptAesGcm(encryptionBytes).ToHex();

            return (encrypted, contentJson);
        }

        public (MessageContent, string, string) Deserialize(
            string payload,
            string aesKeyHex,
            string signingKey)
        {
            var decryptionBytes = aesKeyHex.EnsureHexPrefix().HexToByteArray();
            var decryptedBytes = payload.HexToByteArray().DecryptAesGcm(decryptionBytes);

            var signature = decryptedBytes.Take(65).ToArray();
            var body = decryptedBytes.Skip(65).ToArray();

            var bodyString = body.ToHex().HexToUTF8String();
            var message = JsonConvert.DeserializeObject<MessageContent>(bodyString);
            var sig = signature.ToHex();

            if (!_signService.VerifySign(bodyString, sig, signingKey))
                throw new InvalidOperationException($"Signature is not valid for message {JsonConvert.SerializeObject(message.Header, settings: JsonSerializerSettings)}");

            return (message, bodyString, sig);
        }
    }
}