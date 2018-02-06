using System.Configuration;
using System.IO;
using System.Text;

namespace LoggerLibrary.Transports
{
    public class FileTransport : Transport
    {
        public override void SaveLog(string message)
        {
            var path = ConfigurationManager.AppSettings["logPath"];
            if (string.IsNullOrWhiteSpace(path))
                path = "log.log";
            File.AppendAllLines(path, new[] { message }, Encoding.UTF8);
        }
    }
}
