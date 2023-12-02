using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.TestExtensions;

public static class LoggerTestExtensions
{
    public static void AnyLogOfType<T>(this ILogger<T> logger, LogLevel level, string message) where T : class
    {
        logger.Log(
            level,
            Arg.Any<EventId>(),
             Arg.Is<object>(o => o.ToString() == message),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}