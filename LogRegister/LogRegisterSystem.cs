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
    }
}
