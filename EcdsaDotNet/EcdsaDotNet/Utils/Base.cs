using System;

namespace CPSS.EllipticCurve.Utils
{
    public static class Base64
    {
        public static byte[] Decode(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        public static string Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}