using System;

namespace EllipticCurve.Utils
{
    public static class Bytes
    {
        public static byte[] SliceByteArray(byte[] bytes, int start)
        {
            var newLength = bytes.Length - start;
            var result = new byte[newLength];
            Array.Copy(bytes, start, result, 0, newLength);
            return result;
        }

        public static byte[] SliceByteArray(byte[] bytes, int start, int length)
        {
            var newLength = Math.Min(bytes.Length - start, length);
            var result = new byte[newLength];
            Array.Copy(bytes, start, result, 0, newLength);
            return result;
        }

        public static byte[] IntToCharBytes(int num)
        {
            return new[] { (byte)num };
        }
    }
}