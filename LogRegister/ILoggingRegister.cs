namespace LogRegister
{
    public interface ILoggingRegister
    {
        public bool AppearWithoutVerbose { get; }
        public bool AppearInErrors { get; }
        public ILogCategory Category { get; }
        public string MessageType { get; }
        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
