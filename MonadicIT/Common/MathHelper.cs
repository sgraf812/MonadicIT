using System;
using System.Linq;

namespace MonadicIT.Common
{
    public static class MathHelper
    {
        public static int BinomialCoefficient(int n, int k)
        {
            var nOverK = 1;
            for (var i = 1; i <= k; i++)
            {
                nOverK *= n - k + i;
                nOverK /= i;
            }
            return nOverK;
        }

        public static double KOutOfNProbability(int n, int k, double p)
        {
            return BinomialCoefficient(n, k)*Math.Pow(p, k)*Math.Pow(1 - p, n - k);
        }

        public static double Clamp(this double val, double min, double max)
        {
            return Math.Min(max, Math.Max(min, val));
        }
    }
}