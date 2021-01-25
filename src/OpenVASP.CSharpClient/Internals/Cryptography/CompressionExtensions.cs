using System;
using System.Collections.Generic;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace OpenVASP.CSharpClient.Internals.Cryptography
{
    public static class CompressionExtensions
    {
        private const string CurveTableName = "secp256k1";

        public static byte[] DecompressPublicKey(this string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentNullException();

            var publicKeyBytes = publicKey.EnsureHexPrefix().HexToByteArray();
            var ecPubKey = publicKeyBytes.CompressedPublicKeyToEcPublicKey();
            return ecPubKey.ToUncompressedPublicKey();
        }

        private static ECPublicKeyParameters CompressedPublicKeyToEcPublicKey(this byte[] bPubC)
        {
            var pubKey = bPubC.Skip(1).ToArray();

            var curve = ECNamedCurveTable.GetByName(CurveTableName);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var yParity = new BigInteger(bPubC.Take(1).ToArray()).Subtract(BigInteger.Two);
            var x = new BigInteger(1, pubKey);
            var p = ((FpCurve)curve.Curve).Q;
            var a = x.ModPow(new BigInteger("3"), p).Add(new BigInteger("7")).Mod(p);
            var y = a.ModPow(p.Add(BigInteger.One).FloorDivide(new BigInteger("4")), p);

            if (!y.Mod(BigInteger.Two).Equals(yParity))
                y = y.Negate().Mod(p);

            var q = curve.Curve.CreatePoint(x, y);
            return new ECPublicKeyParameters(q, domainParams);
        }

        private static byte[] ByteArrayLeftPad(byte[] input, byte padValue, int len)
        {
            var temp = Enumerable.Repeat(padValue, len).ToArray(); ;
            var startAt = temp.Length - input.Length;
            Array.Copy(input, 0, temp, startAt, input.Length);
            return temp;
        }

        private static byte[] ToUncompressedPublicKey(this AsymmetricKeyParameter ecPublicKey)
        {
            var publicKey = ((ECPublicKeyParameters)ecPublicKey).Q;
            var xs = ByteArrayLeftPad(publicKey.AffineXCoord.ToBigInteger().ToByteArrayUnsigned(), default, 32);
            var ys = ByteArrayLeftPad(publicKey.AffineYCoord.ToBigInteger().ToByteArrayUnsigned(), default, 32);
            return new byte[] { 0x04 }.ConcatMany(xs, ys).ToArray();
        }

        private static BigInteger FloorDivide(this BigInteger a, BigInteger b)
        {
            if (a.CompareTo(BigInteger.Zero) > 0 ^ b.CompareTo(BigInteger.Zero) < 0 && !a.Mod(b).Equals(BigInteger.Zero))
                return a.Divide(b).Subtract(BigInteger.One);

            return a.Divide(b);
        }

        private static IEnumerable<T> ConcatMany<T>(this IEnumerable<T> enumerable, params IEnumerable<T>[] enums)
        {
            return enumerable.Concat(enums.SelectMany(x => x));
        }
    }
}