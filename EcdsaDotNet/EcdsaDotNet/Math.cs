using System.Numerics;
using EllipticCurve.Utils;

namespace EllipticCurve
{
    public static class EcdsaMath
    {
        public static Point Multiply(Point p, BigInteger n, BigInteger order, BigInteger coef, BigInteger primenum)
        {
            //Fast way to multily point and scalar in elliptic curves

            //:param p: First Point to mutiply
            //:param n: Scalar to mutiply
            //: param N: Order of the elliptic curve
            // : param P: Prime number in the module of the equation Y^2 = X ^ 3 + A * X + B(mod p)
            //:param A: Coefficient of the first-order term of the equation Y ^ 2 = X ^ 3 + A * X + B(mod p)
            //:return: Point that represents the sum of First and Second Point

            return FromJacobian(
                JacobianMultiply(
                    ToJacobian(p),
                    n,
                    order,
                    coef,
                    primenum
                ),
                primenum
            );
        }

        public static Point Add(Point p, Point q, BigInteger a, BigInteger pNumber)
        {
            //Fast way to add two points in elliptic curves

            //:param p: First Point you want to add
            //:param q: Second Point you want to add
            //:param P: Prime number in the module of the equation Y^2 = X ^ 3 + A * X + B(mod p)
            //:param A: Coefficient of the first-order term of the equation Y ^ 2 = X ^ 3 + A * X + B(mod p)
            //:return: Point that represents the sum of First and Second Point

            return FromJacobian(
                JacobianAdd(
                    ToJacobian(p),
                    ToJacobian(q),
                    a,
                    pNumber
                ),
                pNumber
            );
        }

        public static BigInteger Inv(BigInteger x, BigInteger n)
        {
            //Extended Euclidean Algorithm.It's the 'division' in elliptic curves

            //:param x: Divisor
            //: param n: Mod for division
            //:return: Value representing the division

            if (x.IsZero) return 0;

            var lm = BigInteger.One;
            var hm = BigInteger.Zero;
            var low = Integer.Modulo(x, n);
            var high = n;

            while (low > 1)
            {
                var r = high / low;

                var nm = hm - lm * r;
                var newLow = high - low * r;

                high = low;
                hm = lm;
                low = newLow;
                lm = nm;
            }

            return Integer.Modulo(lm, n);
        }

        private static Point ToJacobian(Point p)
        {
            //Convert point to Jacobian coordinates

            //: param p: First Point you want to add
            //:return: Point in Jacobian coordinates

            return new Point(p.X, p.Y, 1);
        }

        private static Point FromJacobian(Point p, BigInteger primeNumber)
        {
            //Convert point back from Jacobian coordinates

            //:param p: First Point you want to add
            //:param P: Prime number in the module of the equation Y^2 = X ^ 3 + A * X + B(mod p)
            //:return: Point in default coordinates

            var z = Inv(p.Z, primeNumber);

            return new Point(
                Integer.Modulo(p.X * BigInteger.Pow(z, 2), primeNumber),
                Integer.Modulo(p.Y * BigInteger.Pow(z, 3), primeNumber)
            );
        }

        private static Point JacobianDouble(Point p, BigInteger a, BigInteger primeNum)
        {
            //Double a point in elliptic curves

            //:param p: Point you want to double
            //:param P: Prime number in the module of the equation Y^2 = X ^ 3 + A * X + B(mod p)
            //:param A: Coefficient of the first-order term of the equation Y ^ 2 = X ^ 3 + A * X + B(mod p)
            //:return: Point that represents the sum of First and Second Point

            if (p.Y.IsZero)
                return new Point(
                    BigInteger.Zero,
                    BigInteger.Zero,
                    BigInteger.Zero
                );

            var ysq = Integer.Modulo(
                BigInteger.Pow(p.Y, 2),
                primeNum
            );
            var s = Integer.Modulo(
                4 * p.X * ysq,
                primeNum
            );
            var m = Integer.Modulo(
                3 * BigInteger.Pow(p.X, 2) + a * BigInteger.Pow(p.Z, 4),
                primeNum
            );

            var nx = Integer.Modulo(
                BigInteger.Pow(m, 2) - 2 * s,
                primeNum
            );
            var ny = Integer.Modulo(
                m * (s - nx) - 8 * BigInteger.Pow(ysq, 2),
                primeNum
            );
            var nz = Integer.Modulo(
                2 * p.Y * p.Z,
                primeNum
            );

            return new Point(
                nx,
                ny,
                nz
            );
        }

        private static Point JacobianAdd(Point p, Point q, BigInteger a, BigInteger primeNum)
        {
            // Add two points in elliptic curves

            // :param p: First Point you want to add
            // :param q: Second Point you want to add
            // :param P: Prime number in the module of the equation Y^2 = X^3 + A*X + B (mod p)
            // :param A: Coefficient of the first-order term of the equation Y^2 = X^3 + A*X + B (mod p)
            // :return: Point that represents the sum of First and Second Point

            if (p.Y.IsZero) return q;
            if (q.Y.IsZero) return p;

            var u1 = Integer.Modulo(
                p.X * BigInteger.Pow(q.Z, 2),
                primeNum
            );
            var u2 = Integer.Modulo(
                q.X * BigInteger.Pow(p.Z, 2),
                primeNum
            );
            var s1 = Integer.Modulo(
                p.Y * BigInteger.Pow(q.Z, 3),
                primeNum
            );
            var s2 = Integer.Modulo(
                q.Y * BigInteger.Pow(p.Z, 3),
                primeNum
            );

            if (u1 == u2)
            {
                if (s1 != s2) return new Point(BigInteger.Zero, BigInteger.Zero, BigInteger.One);
                return JacobianDouble(p, a, primeNum);
            }

            var h = u2 - u1;
            var r = s2 - s1;
            var h2 = Integer.Modulo(h * h, primeNum);
            var h3 = Integer.Modulo(h * h2, primeNum);
            var u1H2 = Integer.Modulo(u1 * h2, primeNum);
            var nx = Integer.Modulo(
                BigInteger.Pow(r, 2) - h3 - 2 * u1H2,
                primeNum
            );
            var ny = Integer.Modulo(
                r * (u1H2 - nx) - s1 * h3,
                primeNum
            );
            var nz = Integer.Modulo(
                h * p.Z * q.Z,
                primeNum
            );

            return new Point(
                nx,
                ny,
                nz
            );
        }

        private static Point JacobianMultiply(Point p, BigInteger n, BigInteger order, BigInteger a,
            BigInteger primeNum)
        {
            // Multily point and scalar in elliptic curves

            // :param p: First Point to mutiply
            // :param n: Scalar to mutiply
            // :param N: Order of the elliptic curve
            // :param P: Prime number in the module of the equation Y^2 = X^3 + A*X + B (mod p)
            // :param A: Coefficient of the first-order term of the equation Y^2 = X^3 + A*X + B (mod p)
            // :return: Point that represents the sum of First and Second Point

            if (p.Y.IsZero | n.IsZero)
                return new Point(
                    BigInteger.Zero,
                    BigInteger.Zero,
                    BigInteger.One
                );

            if (n.IsOne) return p;

            if ((n < 0) | (n >= order))
                return JacobianMultiply(
                    p,
                    Integer.Modulo(n, n),
                    order,
                    a,
                    primeNum
                );

            if (Integer.Modulo(n, 2).IsZero)
                return JacobianDouble(
                    JacobianMultiply(
                        p,
                        n / 2,
                        order,
                        a,
                        primeNum
                    ),
                    a,
                    primeNum
                );

            // (n % 2) == 1:
            return JacobianAdd(
                JacobianDouble(
                    JacobianMultiply(
                        p,
                        n / 2,
                        order,
                        a,
                        primeNum
                    ),
                    a,
                    primeNum
                ),
                p,
                a,
                primeNum
            );
        }
    }
}