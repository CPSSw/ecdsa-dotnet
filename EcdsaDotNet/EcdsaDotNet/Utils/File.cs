namespace CPSS.EllipticCurve.Utils
{
    public static class File
    {
        public static string Read(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static byte[] ReadBytes(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }
    }
}