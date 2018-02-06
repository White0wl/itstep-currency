using LoggerLibrary.Transports;
using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace LoggerLibrary
{
    public class Logger
    {
        private static Logger _logger;
        public static Logger Instance => _logger ?? (_logger = new Logger());
        public string Format { get; set; }
        public Transport Transport { get; set; } = new ConsoleTransport();

        public void LogMessage(string message, string type = "info", [CallerFilePath]string callerFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(Format))
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
            Format = ConfigurationManager.AppSettings["logFormat"];
            if (string.IsNullOrWhiteSpace(Format))//[%T] (%D:%t) <%F:%L>:
                Format = "%m";
        }
    }
}
