using System;
using System.Collections.Generic;
using System.Numerics;
using EllipticCurve.Utils;

namespace EllipticCurve
{
    public class Signature
    {
        public Signature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public BigInteger R { get; }
        public BigInteger S { get; }

        public byte[] ToDer()
        {
            var sequence = new List<byte[]> { Der.EncodeInteger(R), Der.EncodeInteger(S) };
            return Der.EncodeSequence(sequence);
        }

        public string ToBase64()
        {
            return Base64.Encode(ToDer());
        }

        public static Signature FromDer(byte[] bytes)
        {
            var removeSequence = Der.RemoveSequence(bytes);
            var rs = removeSequence.Item1;
            var removeSequenceTrail = removeSequence.Item2;

            if (removeSequenceTrail.Length > 0)
                throw new ArgumentException("trailing junk after DER signature: " +
                                            BinaryAscii.HexFromBinary(removeSequenceTrail));

            var removeInteger = Der.RemoveInteger(rs);
            var r = removeInteger.Item1;
            var rest = removeInteger.Item2;

            removeInteger = Der.RemoveInteger(rest);
            var s = removeInteger.Item1;
            var removeIntegerTrail = removeInteger.Item2;

            if (removeIntegerTrail.Length > 0)
                throw new ArgumentException("trailing junk after DER numbers: " +
                                            BinaryAscii.HexFromBinary(removeIntegerTrail));

            return new Signature(r, s);
        }

        public static Signature FromBase64(string str)
        {
            return FromDer(Base64.Decode(str));
        }
    }
}