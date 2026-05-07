namespace LogRegister
{
    public interface ILoggingRegister
    {
        public ILogCategory Category { get; }
        public string MessageType { get; }
        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
