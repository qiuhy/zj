using System;
using System.Collections.Generic;
using System.Reflection;
namespace util
{
    class UsedTime
    {
        Dictionary<string, DateTime> _begTime = new Dictionary<string, DateTime>();
        public UsedTime() => Reset();
        public void Reset() { _begTime.Clear(); Add(""); }

        public TimeSpan GetElapse(string name = "") => DateTime.Now - _begTime[name];
        public void Add(string name) => _begTime.Add(name, DateTime.Now);
        public DateTime BegTime(string name) => _begTime[name];
    }
}