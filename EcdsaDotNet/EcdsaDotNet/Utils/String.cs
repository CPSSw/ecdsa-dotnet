namespace CPSS.EllipticCurve.Utils
{
    public static class String
    {
        public static string Substring(string str, int index, int length)
        {
            if (str.Length > index + length) return str.Substring(index, length);
            return str.Length > index ? str[index..] : "";
        }
    }
}