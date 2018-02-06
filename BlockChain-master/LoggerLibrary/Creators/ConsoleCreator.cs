using LoggerLibrary.Transports;

namespace LoggerLibrary.Creators
{
    internal class ConsoleCreator : Creator
    {
        public override Transport CreateTransport() { return new ConsoleTransport(); }
    }
}
