namespace LogRegister
{
    /// <summary>Stores structured log events in their original insertion order.</summary>
    public class LogRegisterSystem
    {
        private readonly List<ILoggingRegister> _events;
        /// <summary>Initializes an empty log register.</summary>
        public LogRegisterSystem()
        {
            _events = [];
        }
        /// <summary>Adds a structured event to the log.</summary>
        public void Add(ILoggingRegister logRegister)
        {
            _events.Add(logRegister);
        }
        /// <summary>Gets a read-only view of all registered events.</summary>
        public IReadOnlyList<ILoggingRegister> GetRegisters()
        {
            return _events.AsReadOnly();
        }
        /// <summary>Determines whether any event uses an exact category type.</summary>
        public bool HasLogsOfType<T>() where T : ILogCategory
        {
            Type type = typeof(T);
            return _events.Any(e => e.Category.GetType() == type);
        }
    }
}
