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

        public static String Number2Str(object val)
        {
            decimal d = Convert.ToDecimal(val);
            if (d >= 100000000)
                return $"{d / 100000000:f2}亿";
            else if (d >= 10000)
                return $"{d / 10000:f2}万";
            else
                return d.ToString();
        }
    }
}