using System.Numerics;

namespace EllipticCurve
{
    public class Point
    {
        public Point(BigInteger x, BigInteger y, BigInteger? z = null)
        {
            var zeroZ = z ?? BigInteger.Zero;

            X = x;
            Y = y;
            Z = zeroZ;
        }

        public BigInteger X { get; }
        public BigInteger Y { get; }
        public BigInteger Z { get; }
    }
}