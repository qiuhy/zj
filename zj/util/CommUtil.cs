using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace util
{
    public static class CommUtil
    {
        static CommUtil()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch { }
        }

        private static Dictionary<string, int> curID = new Dictionary<string, int>();

        public static int GetCurID(string name = "") => curID.GetValueOrDefault(name, 1);

        public static int GetNextID(string name = "")
        {
            lock (curID)
            {
                if (curID.ContainsKey(name))
                {
                    curID[name] += 1;
                }
                else
                {
                    curID.Add(name, 1);
                }
                return curID[name];
            }
        }

        public static void swap<T>(ref T item1, ref T item2)
        {
            T item0;
            item0 = item1;
            item1 = item2;
            item2 = item0;
            return;
        }

        public static bool isStartWith<T>(T[] a1, T[] a2)
        {
            if (a1.Length < a2.Length) return false;
            for (int i = 0; i < a2.Length; i++)
            {
                if (!a1[i].Equals(a2[i]))
                    return false;
            }
            return true;
        }

        public static Encoding GetDataEncoding(byte[] data, Encoding defaultEncoding = null)
        {
            // Unicode 字节顺序标记 BOM (Byte Ordered Mask) 十六进制格式︰
            // UTF-8:                           EF BB BF
            // Utf-16 big endian 字节顺序︰      FE FF
            // Utf-16 little-endian 字节顺序︰   FF FE
            // Utf-32 big endian 字节顺序︰      00 00 FE FF
            // Utf-32 little-endian 字节顺序︰   FF FE 00 00
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();
                byte[] bom = e.GetPreamble();
                //  一个字节数组，包含指定所用编码的字节序列。
                // - 或 -
                // 长度为零的字节数组（如果不需要前导码）。
                if (bom.Length > 0 && isStartWith(data, bom))
                    return e;
            }
            Encoding[] testEncoding = new Encoding[] {
                                Encoding.UTF7, Encoding.GetEncoding(936),
                                Encoding.UTF8, Encoding.UTF32, Encoding.Unicode };
            foreach (Encoding e in testEncoding)
            {
                byte[] encoded = e.GetBytes(e.GetString(data));
                if (isStartWith(data, encoded))
                    return e;
            }
            return (defaultEncoding == null) ? Encoding.Default : defaultEncoding;

        }
        public static Encoding GetFileEncoding(string filename)
        {
            byte[] data;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                byte[] buf = new byte[1024];
                count = fs.Read(buf, 0, 1024);
                if (count < 1024)
                {
                    data = new byte[count];
                    Array.Copy(buf, data, count);
                }
                else
                {
                    data = buf;
                }
            }
            return GetDataEncoding(data);
        }
    }
}