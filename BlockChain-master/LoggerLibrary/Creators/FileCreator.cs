using LoggerLibrary.Transports;

namespace LoggerLibrary.Creators
{
    public class FileCreator : Creator
    {
        public override Transport CreateTransport() { return new FileTransport(); }
    }
}
