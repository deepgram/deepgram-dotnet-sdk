using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.TestExtensions;

public static class LoggerTestExtensions
{
    public static void AnyLogOfType<T>(this ILogger<T> logger, LogLevel level) where T : class
    {
        logger.Log(
            level,
            Arg.Any<EventId>(),
             Arg.Is<object>(o => o != null),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}