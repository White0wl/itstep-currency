using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace LoggerLibrary.Transports
{
    public class FileTransport : Transport
    {
        public override void SaveLog(string message)
        {
            string path;
            try
            {
                path = ConfigurationManager.AppSettings["logPath"];
            }
            catch (Exception e)
            {
                path = "log.log";
            }
            File.AppendAllLines(path, new string[] { message }, Encoding.UTF8);
        }
    }
}
