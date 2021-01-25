using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using OpenVASP.CSharpClient.Internals.Cryptography;
using Org.BouncyCastle.Math;
using Xunit;
using Xunit.Abstractions;

namespace OpenVASP.Tests
{
    public class EncryptionTest
    {
        [Fact]
        public void SharedSecretGenerationTest()
        {
            X25519Key alice = X25519Key.GenerateKey();
            X25519Key bob = X25519Key.GenerateKey();

            var shared1 = alice.GenerateSharedSecretHex(bob.PublicKey);
            var shared2 = bob.GenerateSharedSecretHex(alice.PublicKey);

            Assert.Equal(shared1, shared2);
        }

        [Fact]
        public void SharedSecretGenerationBasedOnEthKeyImportTest()
        {
            EthECKey ecKey1 = EthECKey.GenerateKey();
            EthECKey ecKey2 = EthECKey.GenerateKey();
            X25519Key alice = X25519Key.ImportKey(ecKey1.GetPrivateKey());
            X25519Key bob = X25519Key.ImportKey(ecKey2.GetPrivateKey());

            var shared1 = alice.GenerateSharedSecretHex(bob.PublicKey);
            var shared2 = bob.GenerateSharedSecretHex(alice.PublicKey);

            Assert.Equal(shared1, shared2);
        }

        [Fact]
        public void SharedSecretECDHGenerationTest()
        {
            ECDH_Key alice = ECDH_Key.GenerateKey();
            ECDH_Key bob = ECDH_Key.GenerateKey();

            var shared1 = alice.GenerateSharedSecretHex(bob.PublicKey);
            var shared2 = bob.GenerateSharedSecretHex(alice.PublicKey);

            Assert.Equal(shared1, shared2);
        }

        [Fact]
        public void GenerateHandshakeKeyForVaspSmartContractTest()
        {
            ECDH_Key alice = ECDH_Key.GenerateKey();
            ECDH_Key importValid = ECDH_Key.ImportKey(alice.PrivateKey);

            Assert.Equal(alice.PrivateKey, importValid.PrivateKey);
            Assert.Equal(alice.PublicKey, importValid.PublicKey);
        }

        [Fact]
        public void SharedSecretECDHImportTest()
        {
            EthECKey ecKey1 = EthECKey.GenerateKey();
            EthECKey ecKey2 = EthECKey.GenerateKey();

            ECDH_Key alice = ECDH_Key.ImportKey(ecKey1.GetPrivateKey());
            ECDH_Key bob = ECDH_Key.ImportKey(ecKey2.GetPrivateKey());

            var ecPubKey1 = ecKey1.GetPubKey().ToHex(true);
            var ecPubKey2 = ecKey2.GetPubKey().ToHex(true);

            var shared1 = alice.GenerateSharedSecretHex(bob.PublicKey);
            var shared2 = bob.GenerateSharedSecretHex(alice.PublicKey);

            Assert.Equal(shared1, shared2);
        }

        [Theory]
        [InlineData("0x3a854321be6027990865a00269c275cb100b79583cb8c44739894dea04062796",
            "0x498ac9f68e156109de2b3da58ded53d60dfa0e606c87460417bff541e300bf8c3a6a2519458c6d3ef7dae2725b7677744c7b07308239c9bba5288f778dcec788",
            "0x46bdbcc3218d12563e3ea67aca132eab9b34880fd8e3ae65a951572a0b388859")]
        public void ValidateEcdhKeysSharedKeyGenerationForPubKeyTest(string privateKeySecp256k1, string publicKeySecp256k1, string sharedKey)
        {
            ECDH_Key alice = ECDH_Key.ImportKey(privateKeySecp256k1);

            var shared1 = alice.GenerateSharedSecretHex(publicKeySecp256k1);

            Assert.Equal(sharedKey, shared1);
        }

        //https://asecuritysite.com/encryption/ecdh2
        [Theory]
        [InlineData("17742567094485903526920495776528639863119825830619626212595244443740992681934",
            "85498408706124491462794808557977539383806451984214836600380270129359495836802",
            "25333648505365712086758569151951897083364467360470556816024545717425537010350")]
        [InlineData("5305794200459280019319687065362875423514934355333746900115355553495215142023",
            "38765522014560502073188713829594007905335270619209874215410252913414868494979",
            "25217328061577606198811048516258905095331656236019434619572214238361874592068L")]
        public void ValidateEcdhKeysSharedKeyGenerationForPrivateKeyTest(string aliceKeyStr, string bobKeyStr, string expectedSharedStr)
        {
            BigInteger aliceKey = new BigInteger(aliceKeyStr.Replace("L", ""));
            BigInteger bobKey = new BigInteger(bobKeyStr.Replace("L", ""));
            var aliceConverted = aliceKey.ToByteArray().ToHex(true);
            var bobConverted = bobKey.ToByteArray().ToHex(true);

            ECDH_Key alice = ECDH_Key.ImportKey(aliceConverted);
            ECDH_Key bob = ECDH_Key.ImportKey(bobConverted);

            var shared1 = alice.GenerateSharedSecretHex(bob.PublicKey);
            var shared2 = bob.GenerateSharedSecretHex(alice.PublicKey);

            var res = new BigInteger(shared1.HexToByteArray()).ToString();

            Assert.Equal(shared1, shared2);
            Assert.Equal(expectedSharedStr.Replace("L", ""), res);
        }
    }
}
