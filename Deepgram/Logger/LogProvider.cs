using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deepgram.Logger;

public class LogProvider
{
    static IDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
    private static ILoggerFactory _loggerFactory = new LoggerFactory();

    public static void SetLogFactory(ILoggerFactory factory)
    {
        _loggerFactory?.Dispose();
        _loggerFactory = factory;
        _loggers.Clear();
    }

    public static ILogger GetLogger(string category)
    {
        if (!_loggers.TryGetValue(category, out var value))
        {
            value = _loggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
            _loggers[category] = value;
        }
        return value;
    }
}
