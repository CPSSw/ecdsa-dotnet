using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using CPSS.EllipticCurve.Utils;

namespace CPSS.EllipticCurve
{
    public sealed class Ecdsa
    {
        public enum HashType
        {
            Sha256, Sha384, Sha512
        }

        private readonly HashType _hashType;

        private static HashAlgorithm CreateHashAlgorithm(HashType hash)
        {
            return hash switch
            {
                HashType.Sha256 => SHA256.Create(),
                HashType.Sha384 => SHA384.Create(),
                HashType.Sha512 => SHA512.Create(),
                _ => SHA256.Create()
            };
        }
        
        
        public Ecdsa(HashType algorithmName)
        {
            _hashType = algorithmName;
        }
        
        public Signature Sign(string message, PrivateKey privateKey)
        {
            var hashMessage = Hash(message);
            var numberMessage = BinaryAscii.NumberFromHex(hashMessage);
            var curve = privateKey.Curve;
            var randNum = Integer.RandomBetween(BigInteger.One, curve.N - 1);
            var randSignPoint = EcdsaMath.Multiply(curve.G, randNum, curve.N, curve.A, curve.P);
            var r = Integer.Modulo(randSignPoint.X, curve.N);
            var s = Integer.Modulo((numberMessage + r * privateKey.Secret) * EcdsaMath.Inv(randNum, curve.N), curve.N);

            return new Signature(r, s);
        }

        public bool Verify(string message, Signature signature, PublicKey publicKey)
        {
            var hashMessage = Hash(message);
            var numberMessage = BinaryAscii.NumberFromHex(hashMessage);
            var curve = publicKey.Curve;
            var sigR = signature.R;
            var sigS = signature.S;
            var inv = EcdsaMath.Inv(sigS, curve.N);

            var u1 = EcdsaMath.Multiply(
                curve.G,
                Integer.Modulo(numberMessage * inv, curve.N),
                curve.N,
                curve.A,
                curve.P
            );
            var u2 = EcdsaMath.Multiply(
                publicKey.Point,
                Integer.Modulo(sigR * inv, curve.N),
                curve.N,
                curve.A,
                curve.P
            );
            var add = EcdsaMath.Add(
                u1,
                u2,
                curve.A,
                curve.P
            );

            return sigR == add.X;
        }
        
        private string Hash(string message)
        {
            using var shaHash = CreateHashAlgorithm(_hashType);
            var bytes = shaHash.ComputeHash(Encoding.UTF8.GetBytes(message));
            var builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}