using System;
using System.Collections.Generic;

namespace util
{
    public static class util
    {
        private static Dictionary<string, int> curID = new Dictionary<string, int>();
        public static void swap<T>(ref T item1, ref T item2)
        {
            T item0;
            item0 = item1;
            item1 = item2;
            item2 = item0;
            return;
        }
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
        public static int GetCurID(string name = "") => curID.GetValueOrDefault(name, 1);
    }
}