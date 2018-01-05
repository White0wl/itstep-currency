using System;

namespace LoggerLibrary.Transports
{
    public class ConsoleTransport : Transport
    {
        public override void SaveLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}
