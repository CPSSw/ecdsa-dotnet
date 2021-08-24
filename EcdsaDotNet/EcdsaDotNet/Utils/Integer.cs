using System;
using System.Numerics;
using System.Security.Cryptography;

namespace CPSS.EllipticCurve.Utils
{
    public static class Integer
    {
        public static BigInteger Modulo(BigInteger dividend, BigInteger divisor)
        {
            var remainder = BigInteger.Remainder(dividend, divisor);

            if (remainder < 0) return remainder + divisor;

            return remainder;
        }

        public static BigInteger RandomBetween(BigInteger minimum, BigInteger maximum)
        {
            while (true)
            {
                if (maximum < minimum) throw new ArgumentException("maximum must be greater than minimum");

                var range = maximum - minimum;

                var (bytesNeeded, mask) = CalculateParameters(range);

                var randomBytes = new byte[bytesNeeded];
                using (var random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(randomBytes);
                }

                var randomValue = new BigInteger(randomBytes);

                /* We apply the mask to reduce the amount of attempts we might need
                * to make to get a number that is in range. This is somewhat like
                * the commonly used 'modulo trick', but without the bias:
                *
                *   "Let's say you invoke secure_rand(0, 60). When the other code
                *    generates a random integer, you might get 243. If you take
                *    (243 & 63)-- noting that the mask is 63-- you get 51. Since
                *    51 is less than 60, we can return this without bias. If we
                *    got 255, then 255 & 63 is 63. 63 > 60, so we try again.
                *
                *    The purpose of the mask is to reduce the number of random
                *    numbers discarded for the sake of ensuring an unbiased
                *    distribution. In the example above, 243 would discard, but
                *    (243 & 63) is in the range of 0 and 60."
                *
                *   (Source: Scott Arciszewski)
                */

                randomValue &= mask;

                if (randomValue <= range) /* We've been working with 0 as a starting point, so we need to
                    * add the `minimum` here. */
                    return minimum + randomValue;

                /* Outside of the acceptable range, throw it away and try again.
                * We don't try any modulo tricks, as this would introduce bias. */
            }
        }

        private static Tuple<int, BigInteger> CalculateParameters(BigInteger range)
        {
            var bitsNeeded = 0;
            var bytesNeeded = 0;
            var mask = new BigInteger(1);

            while (range > 0)
            {
                if (bitsNeeded % 8 == 0) bytesNeeded += 1;

                bitsNeeded++;

                mask = (mask << 1) | 1;

                range >>= 1;
            }

            return Tuple.Create(bytesNeeded, mask);
        }
    }
}