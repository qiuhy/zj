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

        public static String List2Str<T>(ICollection<T> l)
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
            return sb.ToString();
        }
    }
}