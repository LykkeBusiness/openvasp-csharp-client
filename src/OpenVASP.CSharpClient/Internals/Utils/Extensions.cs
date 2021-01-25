using System;
using System.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace OpenVASP.CSharpClient.Internals.Utils
{
    public static class Extensions
    {
        public static string ToStandardHex(this string s)
        {
            return s.StartsWith("0x04") ? s : $"0x04{s}";
        }
        
        public static string ToOpenVaspHex(this string s)
        {
            return s.StartsWith("0x04") ? s.Substring(4) : s;
        }
        
        public static byte[] DecryptAesGcm(this byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
        {
            var keySize = 256;
            var macSize = 128;
            var nonceSize = 96;
            
            if (key == null || key.Length != keySize / 8)
            {
                throw new ArgumentException(String.Format("Key needs to be {0} bit! actual:{1}", keySize, key?.Length * 8), "key");
            }

            if (encryptedMessage == null || encryptedMessage.Length == 0)
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            using (var cipherStream = new MemoryStream(encryptedMessage))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                //Grab Payload
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                //Grab Nonce
                var nonce = cipherReader.ReadBytes(nonceSize / 8);

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), macSize, nonce, nonSecretPayload);
                cipher.Init(false, parameters);

                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                cipher.DoFinal(plainText, len);

                return plainText;
            }
        }

        public static byte[] EncryptAesGcm(this byte[] messageToEncrypt, byte[] key, byte[] nonSecretPayload = null)
        {
            var keySize = 256;
            var macSize = 128;
            var nonceSize = 96;
            
            if (key == null || key.Length != keySize / 8)
            {
                throw new ArgumentException(String.Format("Key needs to be {0} bit! actual:{1}", keySize, key?.Length * 8), "key");
            }

            //Non-secret Payload Optional
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            //Using random nonce large enough not to repeat
            var nonce = new byte[nonceSize / 8];
            new SecureRandom().NextBytes(nonce, 0, nonce.Length);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), macSize, nonce, nonSecretPayload);
            cipher.Init(true, parameters);

            //Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(messageToEncrypt.Length)];
            var len = cipher.ProcessBytes(messageToEncrypt, 0, messageToEncrypt.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            //Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    //Prepend Authenticated Payload
                    binaryWriter.Write(nonSecretPayload);
                    //Prepend Nonce
                    binaryWriter.Write(nonce);
                    //Write Cipher Text
                    binaryWriter.Write(cipherText);
                }
                return combinedStream.ToArray();
            }
        }
    }
}