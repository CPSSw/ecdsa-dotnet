using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using EllipticCurve.Utils;

namespace EllipticCurve
{
    public static class Ecdsa
    {
        public static Signature Sign(string message, PrivateKey privateKey)
        {
            var hashMessage = Sha256(message);
            var numberMessage = BinaryAscii.NumberFromHex(hashMessage);
            var curve = privateKey.Curve;
            var randNum = Integer.RandomBetween(BigInteger.One, curve.N - 1);
            var randSignPoint = EcdsaMath.Multiply(curve.G, randNum, curve.N, curve.A, curve.P);
            var r = Integer.Modulo(randSignPoint.X, curve.N);
            var s = Integer.Modulo((numberMessage + r * privateKey.Secret) * EcdsaMath.Inv(randNum, curve.N), curve.N);

            return new Signature(r, s);
        }

        public static bool Verify(string message, Signature signature, PublicKey publicKey)
        {
            var hashMessage = Sha256(message);
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

        private static string Sha256(string message)
        {
            byte[] bytes;

            using (var sha256Hash = SHA256.Create())
            {
                bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(message));
            }

            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }
    }
}