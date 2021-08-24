using System;
using System.Collections.Generic;
using System.Numerics;
using EllipticCurve.Utils;

namespace EllipticCurve
{
    public class CurveFp
    {
        public CurveFp(BigInteger a, BigInteger b, BigInteger p, BigInteger n, BigInteger gx, BigInteger gy,
            string name, int[] oid, string nistName = "")
        {
            A = a;
            B = b;
            P = p;
            N = n;
            G = new Point(gx, gy);
            Name = name;
            NistName = nistName;
            Oid = oid;
        }

        public BigInteger A { get; }
        public BigInteger B { get; }
        public BigInteger P { get; }
        public BigInteger N { get; }
        public Point G { get; }
        public string Name { get; }
        public int[] Oid { get; }
        public string NistName { get; }

        public bool Contains(Point p)
        {
            return Integer.Modulo(
                BigInteger.Pow(p.Y, 2) - (BigInteger.Pow(p.X, 3) + A * p.X + B),
                P
            ).IsZero;
        }

        public int Length()
        {
            return N.ToString("X").Length / 2;
        }
    }

    public static class Curves
    {
        public static CurveFp Secp256K1 = new(
            BinaryAscii.NumberFromHex("0000000000000000000000000000000000000000000000000000000000000000"),
            BinaryAscii.NumberFromHex("0000000000000000000000000000000000000000000000000000000000000007"),
            BinaryAscii.NumberFromHex("fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f"),
            BinaryAscii.NumberFromHex("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141"),
            BinaryAscii.NumberFromHex("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798"),
            BinaryAscii.NumberFromHex("483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8"),
            "secp256k1",
            new[] { 1, 3, 132, 0, 10 }
        );

        public static CurveFp Prime256V1 = new(
            BinaryAscii.NumberFromHex("ffffffff00000001000000000000000000000000fffffffffffffffffffffffc"),
            BinaryAscii.NumberFromHex("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b"),
            BinaryAscii.NumberFromHex("ffffffff00000001000000000000000000000000ffffffffffffffffffffffff"),
            BinaryAscii.NumberFromHex("ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551"),
            BinaryAscii.NumberFromHex("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"),
            BinaryAscii.NumberFromHex("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5"),
            "prime256v1",
            new[] { 1, 2, 840, 10045, 3, 1, 7 },
            "P-256"
        );

        public static CurveFp P256 = Prime256V1;

        public static CurveFp[] SupportedCurves = { Secp256K1, Prime256V1 };

        public static Dictionary<string, CurveFp> CurvesByOid = new()
        {
            { string.Join(",", Secp256K1.Oid), Secp256K1 },
            { string.Join(",", Prime256V1.Oid), Prime256V1 }
        };

        public static CurveFp GetCurveByName(string name)
        {
            name = name.ToLower();

            if (name == "secp256k1") return Secp256K1;
            if ((name == "p256") | (name == "prime256v1")) return Prime256V1;

            throw new ArgumentException("unknown curve " + name);
        }
    }
}