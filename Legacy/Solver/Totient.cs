using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public static class MathN
    {
        public static long[] CalcTotients(long n)
        {
            long[] divisors = GetDivisors(n);
            long i;
            var phi = new long[n];
            phi[1] = 1;
            for (i = 1; i < n; ++i)
                CalcTotient(i, phi, divisors);

            return phi;
        }

        /// <summary>
        /// For every integer, the result will contain its lowest divisor.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long[] GetDivisors(long n)
        {
            var divisors = new long[n];
            divisors[1] = 1;
            long i;
            for (i = 2; i < n; ++i)
            {
                if (divisors[i] != 0)
                    continue;

                for (long j = i; j < n; j += i)
                    divisors[j] = i;
            }
            return divisors;
        }

        private static long CalcTotient(long i, long[] phi, long[] divisors)
        {
            if (phi[i] != 0)
                return phi[i];

            long div = divisors[i];
            if (div == i)
            {
                phi[i] = i - 1;
                return phi[i];
            }

            long lower = 1;
            int exp = 0;
            while ((i > 1) && (i % div == 0))
            {
                i /= div;
                lower *= div;
                exp++;
            }
            if (i == 1)
            {
                phi[lower] = ((long)System.Math.Pow(div, exp - 1)) * (div - 1);
                return phi[lower];
            }
            phi[i * lower] = CalcTotient(i, phi, divisors) *
                                 CalcTotient(lower, phi, divisors);
            return phi[i * lower];
        }


        public static int[] GetMu(int max)
        {
            var sqrt = (int)Math.Floor(Math.Sqrt(max));
            var mu = new int[max + 1];
            for (int i = 1; i <= max; i++)
                mu[i] = 1;
            for (int i = 2; i <= sqrt; i++)
            {
                if (mu[i] == 1)
                {
                    for (int j = i; j <= max; j += i)
                        mu[j] *= -i;
                    for (int j = i * i; j <= max; j += i * i)
                        mu[j] = 0;
                }
            }
            for (int i = 2; i <= max; i++)
            {
                if (mu[i] == i)
                    mu[i] = 1;
                else if (mu[i] == -i)
                    mu[i] = -1;
                else if (mu[i] < 0)
                    mu[i] = 1;
                else if (mu[i] > 0)
                    mu[i] = -1;
            }
            return mu;
        }
    }
}
