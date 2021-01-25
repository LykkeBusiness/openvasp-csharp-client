using System;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace OpenVASP.CSharpClient.Internals.Cryptography
{
    public class X25519Key
    {
        private readonly AsymmetricCipherKeyPair _key;

        private X25519Key(AsymmetricCipherKeyPair key)
        {
            _key = key;

            PrivateKey = GetPrivateKey(_key);
            PublicKey = GetPublicKey(_key);
        }

        public string PrivateKey { get; }

        public string PublicKey { get; }

        public string GenerateSharedSecretHex(string pubKeyHex)
        {
            X25519PrivateKeyParameters pkForEcsh = (X25519PrivateKeyParameters)_key.Private;
            byte[] result = new byte[32];
            pkForEcsh.GenerateSecret(new X25519PublicKeyParameters(pubKeyHex.HexToByteArray(), 0), result, 0);

            return result.ToHex(prefix: true);
        }

        public static X25519Key GenerateKey()
        {
            var random = new SecureRandom();
            var generator = GeneratorUtilities.GetKeyPairGenerator("X25519");
            generator.Init(new X25519KeyGenerationParameters(random));
            var generatedKey = generator.GenerateKeyPair();
            var key = new X25519Key(generatedKey);

            return key;
        }

        public static X25519Key ImportKey(string privateKeyHex)
        {
            X25519PrivateKeyParameters privateKey = new X25519PrivateKeyParameters(privateKeyHex.HexToByteArray(), 0);
            X25519PublicKeyParameters publicKey = privateKey.GeneratePublicKey();
            var importedKey = new AsymmetricCipherKeyPair(publicKey, privateKey);
            var key = new X25519Key(importedKey);

            return key;
        }

        private static string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            if (keyPair.Public is X25519PublicKeyParameters dhPublicKeyParameters)
            {
                return dhPublicKeyParameters.GetEncoded().ToHex(prefix: true);
            }
            throw new NullReferenceException("The key pair provided is not a valid ECDH keypair.");
        }

        // This returns a
        private static string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            if (keyPair.Private is X25519PrivateKeyParameters dhPrivateKeyParameters)
            {
                return dhPrivateKeyParameters.GetEncoded().ToHex(prefix: true);
            }
            throw new NullReferenceException("The key pair provided is not a valid ECDH keypair.");
        }
    }
}