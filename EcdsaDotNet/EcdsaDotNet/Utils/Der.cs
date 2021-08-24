using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace EllipticCurve.Utils
{
    public static class Der
    {
        private static readonly int Hex31 = 0x1f;
        private static readonly int Hex127 = 0x7f;
        private static readonly int Hex128 = 0x80;
        private static readonly int Hex160 = 0xa0;
        private static readonly int Hex224 = 0xe0;

        private static readonly string HexAt = "00";
        private static readonly string HexB = "02";
        private static readonly string HexC = "03";
        private static readonly string HexD = "04";
        private static readonly string HexF = "06";
        private static readonly string Hex0 = "30";

        private static readonly byte[] BytesHexAt = BinaryAscii.BinaryFromHex(HexAt);
        private static readonly byte[] BytesHexB = BinaryAscii.BinaryFromHex(HexB);
        private static readonly byte[] BytesHexC = BinaryAscii.BinaryFromHex(HexC);
        private static readonly byte[] BytesHexD = BinaryAscii.BinaryFromHex(HexD);
        private static readonly byte[] BytesHexF = BinaryAscii.BinaryFromHex(HexF);
        private static readonly byte[] BytesHex0 = BinaryAscii.BinaryFromHex(Hex0);

        public static byte[] EncodeSequence(List<byte[]> encodedPieces)
        {
            var totalLengthLen = 0;
            foreach (var piece in encodedPieces) totalLengthLen += piece.Length;
            var sequence = new List<byte[]> { BytesHex0, EncodeLength(totalLengthLen) };
            sequence.AddRange(encodedPieces);
            return CombineByteArrays(sequence);
        }

        public static byte[] EncodeInteger(BigInteger x)
        {
            if (x < 0) throw new ArgumentException("x cannot be negative");

            var t = x.ToString("X");

            if (t.Length % 2 == 1) t = "0" + t;

            var xBytes = BinaryAscii.BinaryFromHex(t);

            int num = xBytes[0];

            if (num <= Hex127)
                return CombineByteArrays(new List<byte[]>
                {
                    BytesHexB,
                    Bytes.IntToCharBytes(xBytes.Length),
                    xBytes
                });

            return CombineByteArrays(new List<byte[]>
            {
                BytesHexB,
                Bytes.IntToCharBytes(xBytes.Length + 1),
                BytesHexAt,
                xBytes
            });
        }

        public static byte[] EncodeOid(int[] oid)
        {
            var first = oid[0];
            var second = oid[1];

            if (first > 2) throw new ArgumentException("first has to be <= 2");

            if (second > 39) throw new ArgumentException("second has to be <= 39");

            var bodyList = new List<byte[]>
            {
                Bytes.IntToCharBytes(40 * first + second)
            };

            for (var i = 2; i < oid.Length; i++) bodyList.Add(EncodeNumber(oid[i]));

            var body = CombineByteArrays(bodyList);

            return CombineByteArrays(new List<byte[]>
            {
                BytesHexF,
                EncodeLength(body.Length),
                body
            });
        }

        public static byte[] EncodeBitString(byte[] t)
        {
            return CombineByteArrays(new List<byte[]>
            {
                BytesHexC,
                EncodeLength(t.Length),
                t
            });
        }

        public static byte[] EncodeOctetString(byte[] t)
        {
            return CombineByteArrays(new List<byte[]>
            {
                BytesHexD,
                EncodeLength(t.Length),
                t
            });
        }

        public static byte[] EncodeConstructed(int tag, byte[] value)
        {
            return CombineByteArrays(new List<byte[]>
            {
                Bytes.IntToCharBytes(Hex160 + tag),
                EncodeLength(value.Length),
                value
            });
        }

        public static Tuple<byte[], byte[]> RemoveSequence(byte[] bytes)
        {
            CheckSequenceError(bytes, Hex0, "30");

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var endSeq = 1 + lengthLen + length;

            return new Tuple<byte[], byte[]>(
                Bytes.SliceByteArray(bytes, 1 + lengthLen, length),
                Bytes.SliceByteArray(bytes, endSeq)
            );
        }

        public static Tuple<BigInteger, byte[]> RemoveInteger(byte[] bytes)
        {
            CheckSequenceError(bytes, HexB, "02");

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var numberBytes = Bytes.SliceByteArray(bytes, 1 + lengthLen, length);
            var rest = Bytes.SliceByteArray(bytes, 1 + lengthLen + length);
            int nBytes = numberBytes[0];

            if (nBytes >= Hex128) throw new ArgumentException("first byte of integer must be < 128");

            return new Tuple<BigInteger, byte[]>(
                BinaryAscii.NumberFromHex(BinaryAscii.HexFromBinary(numberBytes)),
                rest
            );
        }

        public static Tuple<int[], byte[]> RemoveObject(byte[] bytes)
        {
            CheckSequenceError(bytes, HexF, "06");

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var body = Bytes.SliceByteArray(bytes, 1 + lengthLen, length);
            var rest = Bytes.SliceByteArray(bytes, 1 + lengthLen + length);

            var numbers = new List<int>();
            Tuple<int, int> readNumberResult;
            while (body.Length > 0)
            {
                readNumberResult = ReadNumber(body);
                numbers.Add(readNumberResult.Item1);
                body = Bytes.SliceByteArray(body, readNumberResult.Item2);
            }

            var n0 = numbers[0];
            numbers.RemoveAt(0);

            var first = n0 / 40;
            var second = n0 - 40 * first;
            numbers.Insert(0, first);
            numbers.Insert(1, second);

            return new Tuple<int[], byte[]>(
                numbers.ToArray(),
                rest
            );
        }

        public static Tuple<byte[], byte[]> RemoveBitString(byte[] bytes)
        {
            CheckSequenceError(bytes, HexC, "03");

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var body = Bytes.SliceByteArray(bytes, 1 + lengthLen, length);
            var rest = Bytes.SliceByteArray(bytes, 1 + lengthLen + length);

            return new Tuple<byte[], byte[]>(body, rest);
        }

        public static Tuple<byte[], byte[]> RemoveOctetString(byte[] bytes)
        {
            CheckSequenceError(bytes, HexD, "04");

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var body = Bytes.SliceByteArray(bytes, 1 + lengthLen, length);
            var rest = Bytes.SliceByteArray(bytes, 1 + lengthLen + length);

            return new Tuple<byte[], byte[]>(body, rest);
        }

        public static Tuple<int, byte[], byte[]> RemoveConstructed(byte[] bytes)
        {
            var s0 = ExtractFirstInt(bytes);

            if ((s0 & Hex224) != Hex160) throw new ArgumentException("wanted constructed tag (0xa0-0xbf), got " + s0);

            var tag = s0 & Hex31;

            var readLengthResult = ReadLength(Bytes.SliceByteArray(bytes, 1));
            var length = readLengthResult.Item1;
            var lengthLen = readLengthResult.Item2;

            var body = Bytes.SliceByteArray(bytes, 1 + lengthLen, length);
            var rest = Bytes.SliceByteArray(bytes, 1 + lengthLen + length);

            return new Tuple<int, byte[], byte[]>(tag, body, rest);
        }

        public static byte[] FromPem(string pem)
        {
            var split = pem.Split(new[] { "\n" }, StringSplitOptions.None);
            var stripped = new List<string>();

            for (var i = 0; i < split.Length; i++)
            {
                var line = split[i].Trim();
                if (String.Substring(line, 0, 5) != "-----") stripped.Add(line);
            }

            return Base64.Decode(string.Join("", stripped));
        }

        public static string ToPem(byte[] der, string name)
        {
            var b64 = Base64.Encode(der);
            var lines = new List<string> { "-----BEGIN " + name + "-----" };

            var strLength = b64.Length;
            for (var i = 0; i < strLength; i += 64) lines.Add(String.Substring(b64, i, 64));
            lines.Add("-----END " + name + "-----");

            return string.Join("\n", lines);
        }

        public static byte[] CombineByteArrays(List<byte[]> byteArrayList)
        {
            var totalLength = 0;
            foreach (var bytes in byteArrayList) totalLength += bytes.Length;

            var combined = new byte[totalLength];
            var consumedLength = 0;

            foreach (var bytes in byteArrayList)
            {
                Array.Copy(bytes, 0, combined, consumedLength, bytes.Length);
                consumedLength += bytes.Length;
            }

            return combined;
        }

        private static byte[] EncodeLength(int length)
        {
            if (length < 0) throw new ArgumentException("length cannot be negative");

            if (length < Hex128) return Bytes.IntToCharBytes(length);

            var s = length.ToString("X");
            if (s.Length % 2 == 1) s = "0" + s;

            var bytes = BinaryAscii.BinaryFromHex(s);
            var lengthLen = bytes.Length;

            return CombineByteArrays(new List<byte[]>
            {
                Bytes.IntToCharBytes(Hex128 | lengthLen),
                bytes
            });
        }

        private static byte[] EncodeNumber(int n)
        {
            var b128Digits = new List<int>();

            while (n > 0)
            {
                b128Digits.Insert(0, (n & Hex127) | Hex128);
                n >>= 7;
            }

            var b128DigitsCount = b128Digits.Count;

            if (b128DigitsCount == 0)
            {
                b128Digits.Add(0);
                b128DigitsCount++;
            }

            b128Digits[b128DigitsCount - 1] &= Hex127;

            var byteList = new List<byte[]>();

            foreach (var digit in b128Digits) byteList.Add(Bytes.IntToCharBytes(digit));

            return CombineByteArrays(byteList);
        }

        private static Tuple<int, int> ReadLength(byte[] bytes)
        {
            var num = ExtractFirstInt(bytes);

            if ((num & Hex128) == 0) return new Tuple<int, int>(num & Hex127, 1);

            var lengthLen = num & Hex127;

            if (lengthLen > bytes.Length - 1) throw new ArgumentException("ran out of length bytes");

            return new Tuple<int, int>(
                int.Parse(
                    BinaryAscii.HexFromBinary(Bytes.SliceByteArray(bytes, 1, lengthLen)),
                    NumberStyles.HexNumber
                ),
                1 + lengthLen
            );
        }

        private static Tuple<int, int> ReadNumber(byte[] str)
        {
            var number = 0;
            var lengthLen = 0;
            int d;

            while (true)
            {
                if (lengthLen > str.Length) throw new ArgumentException("ran out of length bytes");

                number <<= 7;
                d = str[lengthLen];
                number += d & Hex127;
                lengthLen += 1;
                if ((d & Hex128) == 0) break;
            }

            return new Tuple<int, int>(number, lengthLen);
        }

        private static void CheckSequenceError(byte[] bytes, string start, string expected)
        {
            if (BinaryAscii.HexFromBinary(bytes).Substring(0, start.Length) != start)
                throw new ArgumentException(
                    "wanted sequence " +
                    expected.Substring(0, 2) +
                    ", got " +
                    ExtractFirstInt(bytes).ToString("X")
                );
        }

        private static int ExtractFirstInt(byte[] str)
        {
            return str[0];
        }
    }
}