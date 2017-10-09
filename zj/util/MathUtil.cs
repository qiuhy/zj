using System;
using System.Collections;
using System.Collections.Generic;

namespace util
{
    public static class MathUtil
    {
        //采用查表法，提高阶乘的效率
        private static readonly long[] _factorial = new long[]
        {
            1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800,
            479001600, 6227020800, 87178291200, 1307674368000, 20922789888000,
            355687428096000, 6402373705728000, 121645100408832000, 2432902008176640000
        };

        //阶乘 n!
        public static long Factorial(int n) => n > 0 ? _factorial[n] : 1;

        //排列 p(n,r) = n!/(n-r)! = f(n,n-r)
        public static long PermutationCount(int n, int r) => Factorial(n) / Factorial(n - r);

        //组合 c(n,r) = p(n,r)/r!
        //allowAllLevel: c(n,r) + c(n,r-1) + ... + c(n,1)
        public static long CombinationCount(int n, int r, bool allowAllLevel = false)
        {
            if (n <= 0) return 0;
            if (r < 0) return 0;
            if (r > n)
                if (allowAllLevel) r = n; else return 0;
            long result = combination(n, r);
            while (allowAllLevel && r > 1)
            {
                result += combination(n, --r);
            }
            return result;
        }

        private static long combination(int n, int r)
        {
            if (r > n - r)
                r = n - r;

            // 使用阶乘的算法： PermutationCount(n, r) / Factorial(r)

            // 使用乘法的算法
            long result = 1;
            for (int i = 1; i <= r; i++, n--)
            {
                result = checked(result * n) / i;
            }

            return result;
        }

        public static T Min<T>(params T[] args) where T : IComparable<T>
        {
            T min = args[0];
            for (int i = 1; i < args.Length; i++)
            {
                if (min.CompareTo(args[i]) > 0)
                    min = args[i];
            }
            return min;
        }

        public static T Max<T>(params T[] args) where T : IComparable<T>
        {
            T max = args[0];
            for (int i = 1; i < args.Length; i++)
            {
                if (max.CompareTo(args[i]) < 0)
                    max = args[i];
            }
            return max;
        }
    }
}