using LoggerLibrary.Transports;
using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace LoggerLibrary
{
    public class Logger
    {
        private static Logger logger;
        public static Logger Instance
        {
            get { if (logger is null) logger = new Logger(); return logger; }
        }
        public string Format { get; set; }
        public Transport Transport { get; set; } = new ConsoleTransport();

        public void LogMessage(string message, string type = "info", [CallerFilePath]string callerFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (String.IsNullOrWhiteSpace(Format))
                SetDefaultLogFormat();
            lock (this)
            {
                Transport.SaveLog(Format
                .Replace("%T", type.ToUpper())
                .Replace("%D", DateTime.Now.ToShortDateString())
                .Replace("%t", DateTime.Now.ToShortTimeString())
                .Replace("%F", callerFilePath)
                .Replace("%L", sourceLineNumber + "")
                .Replace("%m", message));
            }
        }

        private void SetDefaultLogFormat()
        {
            try
            {
                Format = ConfigurationManager.AppSettings["logFormat"];
                if (String.IsNullOrWhiteSpace(Format))
                    throw new Exception();
            }
            catch
            {
                //[%T] (%D:%t) <%F:%L>:
                Format = "%m";
            }
        }
    }
}
