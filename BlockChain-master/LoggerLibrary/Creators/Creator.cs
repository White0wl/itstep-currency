using LoggerLibrary.Transports;

namespace LoggerLibrary.Creators
{
    public abstract class Creator
    {
        public abstract Transport CreateTransport();
    }
}
