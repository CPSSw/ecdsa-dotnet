using System;
using System.Collections.Generic;
using System.Numerics;
using CPSS.EllipticCurve.Utils;

namespace CPSS.EllipticCurve
{
    public class PrivateKey
    {
        public PrivateKey(string curve = "secp256k1", BigInteger? secret = null)
        {
            Curve = Curves.GetCurveByName(curve);

            secret ??= Integer.RandomBetween(1, Curve.N - 1);
            Secret = (BigInteger)secret;
        }

        public CurveFp Curve { get; }
        public BigInteger Secret { get; }

        public PublicKey PublicKey()
        {
            var publicPoint = EcdsaMath.Multiply(Curve.G, Secret, Curve.N, Curve.A, Curve.P);
            return new PublicKey(publicPoint, Curve);
        }

        public byte[] ToStringFromNumber()
        {
            return BinaryAscii.StringFromNumber(Secret, Curve.Length());
        }

        public byte[] ToDer()
        {
            var encodedPublicKey = PublicKey().ToString(true);

            return Der.EncodeSequence(
                new List<byte[]>
                {
                    Der.EncodeInteger(1),
                    Der.EncodeOctetString(ToStringFromNumber()),
                    Der.EncodeConstructed(0, Der.EncodeOid(Curve.Oid)),
                    Der.EncodeConstructed(1, encodedPublicKey)
                }
            );
        }

        public string ToPem()
        {
            return Der.ToPem(ToDer(), "EC PRIVATE KEY");
        }

        public static PrivateKey FromPem(string str)
        {
            var split = str.Split(new[] { "-----BEGIN EC PRIVATE KEY-----" }, StringSplitOptions.None);

            if (split.Length != 2) throw new ArgumentException("invalid PEM");

            return FromDer(Der.FromPem(split[1]));
        }

        public static PrivateKey FromDer(byte[] der)
        {
            var (bytes, item2) = Der.RemoveSequence(der);
            if (item2.Length > 0)
                throw new ArgumentException("trailing junk after DER private key: " +
                                            BinaryAscii.HexFromBinary(item2));

            var (item1, bytes1) = Der.RemoveInteger(bytes);
            if (item1 != 1)
                throw new ArgumentException("expected '1' at start of DER private key, got " + item1);

            var removeOctetString = Der.RemoveOctetString(bytes1);
            var privateKeyStr = removeOctetString.Item1;

            var (tag, curveOidString, _) = Der.RemoveConstructed(removeOctetString.Item2);
            if (tag != 0) throw new ArgumentException("expected tag 0 in DER private key, got " + tag);

            var (oidCurve, item3) = Der.RemoveObject(curveOidString);
            if (item3.Length > 0)
                throw new ArgumentException(
                    "trailing junk after DER private key curve_oid: " +
                    BinaryAscii.HexFromBinary(item3)
                );

            var stringOid = string.Join(",", oidCurve);

            if (!Curves.CurvesByOid.ContainsKey(stringOid))
            {
                var numCurves = Curves.SupportedCurves.Length;
                var supportedCurves = new string[numCurves];
                for (var i = 0; i < numCurves; i++) supportedCurves[i] = Curves.SupportedCurves[i].Name;
                throw new ArgumentException(
                    "Unknown curve with oid [" +
                    string.Join(", ", oidCurve) +
                    "]. Only the following are available: " +
                    string.Join(", ", supportedCurves)
                );
            }

            var curve = Curves.CurvesByOid[stringOid];

            if (privateKeyStr.Length >= curve.Length()) return FromString(privateKeyStr, curve.Name);
            var length = curve.Length() - privateKeyStr.Length;
            var padding = "";
            for (var i = 0; i < length; i++) padding += "00";
            privateKeyStr = Der.CombineByteArrays(new List<byte[]>
                { BinaryAscii.BinaryFromHex(padding), privateKeyStr });

            return FromString(privateKeyStr, curve.Name);
        }

        public static PrivateKey FromString(byte[] str, string curve = "secp256k1")
        {
            return new PrivateKey(curve, BinaryAscii.NumberFromString(str));
        }
    }
}