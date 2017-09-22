// #define TRACE
using System;
using System.IO;
using System.Diagnostics;

namespace util
{
    class Logger
    {
        private TraceSource _ts = null;
        private string _format = "{0:HH:mm:ss fff} {1}:{2}";
        private int consoleHashCode = 0;
        public Logger(string name = "", string format = "", SourceLevels logLevel = SourceLevels.All, bool log2File = false)
        {
            if ("".Equals(name))
            {
                name = this.GetType().Assembly.GetName().Name;
            }
            if (!"".Equals(format))
            {
                _format = format;
            }

            _ts = new TraceSource(name);
            _ts.Switch.Level = logLevel;
            _ts.Listeners.Clear();
            _ts.Listeners.Add(new TextWriterTraceListener(Console.Out));
            consoleHashCode = _ts.Listeners[0].GetHashCode();
            if (log2File)
            {
                string logFile = String.Format("log\\{0}_{1}.log", name, DateTime.Now.Ticks);
                _ts.Listeners.Add(new TextWriterTraceListener(File.CreateText(logFile)));
                verbose("Logger init :{0}", logFile);
            }
        }

        public void trace(TraceEventType type, string message)
        {
            string strMsg = "";
            if (!"".Equals(message))
                strMsg = String.Format(_format, DateTime.Now, type.ToString()[0], message);
            foreach (TraceListener tl in _ts.Listeners)
            {
                if (tl.GetHashCode() != consoleHashCode || _ts.Switch.ShouldTrace(type))
                {
                    tl.WriteLine(strMsg);
                    tl.Flush();
                }
            }
        }

        public void verbose(Object message) => trace(TraceEventType.Verbose, message.ToString());
        public void verbose(string format, params object[] args) => trace(TraceEventType.Verbose, String.Format(format, args));
        public void info(Object message) => trace(TraceEventType.Information, message.ToString());
        public void info(string format, params object[] args) => trace(TraceEventType.Information, String.Format(format, args));
        public void warning(Object message) => trace(TraceEventType.Warning, message.ToString());
        public void warning(string format, params object[] args) => trace(TraceEventType.Warning, String.Format(format, args));
        public void error(Object message) => trace(TraceEventType.Error, message.ToString());
        public void error(string format, params object[] args) => trace(TraceEventType.Error, String.Format(format, args));
    }
}