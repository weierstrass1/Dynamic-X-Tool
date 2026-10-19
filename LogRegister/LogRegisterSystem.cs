namespace LogRegister
{
    public class LogRegisterSystem
    {
        private readonly List<ILoggingRegister> _events;
        public LogRegisterSystem()
        {
            _events = [];
        }
        public void Add(ILoggingRegister logRegister)
        {
            _events.Add(logRegister);
        }
        public IReadOnlyList<ILoggingRegister> GetRegisters()
        {
            return _events.AsReadOnly();
        }
        public bool HasLogsOfType<T>() where T : ILogCategory
        {
            Type type = typeof(T);
            return _events.Any(e => e.Category.GetType() == type);
        }
    }
}
