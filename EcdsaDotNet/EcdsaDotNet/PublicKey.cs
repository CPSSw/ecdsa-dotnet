using System;
using System.Collections.Generic;
using EllipticCurve.Utils;

namespace EllipticCurve
{
    public class PublicKey
    {
        public PublicKey(Point point, CurveFp curve)
        {
            Point = point;
            Curve = curve;
        }

        public Point Point { get; }

        public CurveFp Curve { get; }

        public byte[] ToString(bool encoded = false)
        {
            var xString = BinaryAscii.StringFromNumber(Point.X, Curve.Length());
            var yString = BinaryAscii.StringFromNumber(Point.Y, Curve.Length());

            if (encoded)
                return Der.CombineByteArrays(new List<byte[]>
                {
                    BinaryAscii.BinaryFromHex("00"),
                    BinaryAscii.BinaryFromHex("04"),
                    xString,
                    yString
                });
            return Der.CombineByteArrays(new List<byte[]>
            {
                xString,
                yString
            });
        }

        public byte[] ToDer()
        {
            int[] oidEcPublicKey = { 1, 2, 840, 10045, 2, 1 };
            var encodedEcAndOid = Der.EncodeSequence(
                new List<byte[]>
                {
                    Der.EncodeOid(oidEcPublicKey),
                    Der.EncodeOid(Curve.Oid)
                }
            );

            return Der.EncodeSequence(
                new List<byte[]>
                {
                    encodedEcAndOid,
                    Der.EncodeBitString(ToString(true))
                }
            );
        }

        public string ToPem()
        {
            return Der.ToPem(ToDer(), "PUBLIC KEY");
        }

        public static PublicKey FromPem(string pem)
        {
            return FromDer(Der.FromPem(pem));
        }

        public static PublicKey FromDer(byte[] der)
        {
            var removeSequence1 = Der.RemoveSequence(der);
            var s1 = removeSequence1.Item1;

            if (removeSequence1.Item2.Length > 0)
                throw new ArgumentException(
                    "trailing junk after DER public key: " +
                    BinaryAscii.HexFromBinary(removeSequence1.Item2)
                );

            var removeSequence2 = Der.RemoveSequence(s1);
            var s2 = removeSequence2.Item1;
            var pointBitString = removeSequence2.Item2;

            var removeObject1 = Der.RemoveObject(s2);
            var rest = removeObject1.Item2;

            var removeObject2 = Der.RemoveObject(rest);
            var oidCurve = removeObject2.Item1;

            if (removeObject2.Item2.Length > 0)
                throw new ArgumentException(
                    "trailing junk after DER public key objects: " +
                    BinaryAscii.HexFromBinary(removeObject2.Item2)
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

            var removeBitString = Der.RemoveBitString(pointBitString);
            var pointString = removeBitString.Item1;

            if (removeBitString.Item2.Length > 0)
                throw new ArgumentException("trailing junk after public key point-string");

            return FromString(Bytes.SliceByteArray(pointString, 2), curve.Name);
        }

        public static PublicKey FromString(byte[] str, string curve = "secp256k1", bool validatePoint = true)
        {
            var curveObject = Curves.GetCurveByName(curve);

            var baseLen = curveObject.Length();

            if (str.Length != 2 * baseLen)
                throw new ArgumentException("string length [" + str.Length + "] should be " + 2 * baseLen);

            var xs = BinaryAscii.HexFromBinary(Bytes.SliceByteArray(str, 0, baseLen));
            var ys = BinaryAscii.HexFromBinary(Bytes.SliceByteArray(str, baseLen));

            var p = new Point(
                BinaryAscii.NumberFromHex(xs),
                BinaryAscii.NumberFromHex(ys)
            );

            if (validatePoint & !curveObject.Contains(p))
                throw new ArgumentException(
                    "point (" +
                    p.X +
                    ", " +
                    p.Y +
                    ") is not valid for curve " +
                    curveObject.Name
                );

            return new PublicKey(p, curveObject);
        }
    }
}