using LoggerLibrary.Transports;

namespace LoggerLibrary.Creators
{
    class ConsoleCreator : Creator
    {
        public override Transport CreateTransport() { return new ConsoleTransport(); }
    }
}
