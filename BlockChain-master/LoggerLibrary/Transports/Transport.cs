namespace LoggerLibrary.Transports
{
    public abstract class Transport
    {
        public abstract void SaveLog(string message);
    }
}
