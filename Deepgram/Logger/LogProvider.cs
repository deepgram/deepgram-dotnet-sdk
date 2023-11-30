namespace Deepgram.Logger
{
    public class LogProvider
    {
        private static readonly ConcurrentDictionary<string, ILogger> _loggers = new();
        private static ILoggerFactory _loggerFactory = new LoggerFactory();

        public static void SetLogFactory(ILoggerFactory factory)
        {
            _loggerFactory?.Dispose();
            _loggerFactory = factory;
            _loggers.Clear();
        }

        public static ILogger GetLogger(string category)
        {
            // Try to get the logger from the dictionary
            if (!_loggers.TryGetValue(category, out var logger))
            {
                // If not found, create a new logger and add it to the dictionary
                logger = _loggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
                _loggers[category] = logger;
            }
            // Return the logger
            return logger;
        }

    }
}
