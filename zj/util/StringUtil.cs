using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace util
{
    public static class StringUtil
    {
        public static String Array2Str<T>(T[] a)
        {
            if (a == null)
                return "";
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < a.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");
                sb.Append(a[i].ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static String Array2Str<T>(IEnumerable<T> l)
        {
            StringBuilder sb = new StringBuilder("[");
            bool bAppend = false;
            foreach (var item in l)
            {
                if (bAppend)
                    sb.Append(",");
                else
                    bAppend = true;
                sb.Append(item.ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static String Number2Str(object val, int decimals = 0)
        {
            decimal d = Convert.ToDecimal(val);
            if (d >= 100000000)
                return $"{d / 100000000:f2}亿";
            else if (d >= 10000)
                return $"{d / 10000:f2}万";
            else if (decimals >= 0)
                return Math.Round(d, decimals).ToString();
            else
                return d.ToString();
        }

        //编辑距离 Levenshtein Distance
        public static int EditDistance(string str1, string str2)
        {

            int rows = str1.Length;
            int cols = str2.Length;
            if (rows == 0) return cols;
            if (cols == 0) return rows;

            int[,] matrix = new int[rows + 1, cols + 1];
            int cost = 0;
            char[] a1 = str1.ToCharArray();
            char[] a2 = str2.ToCharArray();


            for (int i = 0; i < cols; i++)
            {
                matrix[0, i] = i;
            }
            for (int i = 0; i < rows; i++)
            {
                matrix[i, 0] = i;
            }
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    cost = a1[r - 1].Equals(a2[c - 1]) ? 0 : 1;
                    matrix[r, c] = MathUtil.Min(matrix[r - 1, c] + 1, matrix[r, c - 1] + 1, matrix[r - 1, c - 1] + cost);
                }
            }
            return matrix[rows - 1, cols - 1];
        }
        //编辑距离相似度  1-EditDistance/(max(len(a),len(b)))
        public static double SimilarEditDistance(string str1, string str2)
        {
            return 1 - (double)EditDistance(str1, str2) / Math.Max(str1.Length, str2.Length);
        }

        //余弦相似度 结合分词效果更佳
        public static double SimilarCos(string str1, string str2)
        {

            // # 对两个要计算的字符串进行分词
            // # 列出所有词
            // all_words = set(str1 + str2)
            // # 计算词频
            // freq_str1 = [str1.count(x) for x in all_words]
            // freq_str2 = [str2.count(x) for x in all_words]
            // # 计算相似度
            // sum_all = sum(map(lambda z, y: z * y, freq_str1, freq_str2))
            // sqrt_str1 = math.sqrt(sum(x ** 2 for x in freq_str1))
            // sqrt_str2 = math.sqrt(sum(x ** 2 for x in freq_str2))
            // return sum_all / (sqrt_str1 * sqrt_str2)

            HashSet<char> allChars = new HashSet<char>();
            Dictionary<char, int> f1 = new Dictionary<char, int>();
            Dictionary<char, int> f2 = new Dictionary<char, int>();
            foreach (char c in str1)
            {
                allChars.Add(c);
                f1[c] = f1.GetValueOrDefault(c, 0) + 1;
            }
            foreach (char c in str2)
            {
                allChars.Add(c);
                f2[c] = f2.GetValueOrDefault(c, 0) + 1;
            }
            int sum0 = 0, sum1 = 0, sum2 = 0;
            foreach (char c in allChars)
            {
                int i1 = f1.GetValueOrDefault(c, 0);
                int i2 = f2.GetValueOrDefault(c, 0);
                sum0 += i1 * i2;
                sum1 += i1 * i1;
                sum2 += i2 * i2;
            }
            return Math.Sqrt((double)sum0 / (sum1 * sum2));
        }
    }
}