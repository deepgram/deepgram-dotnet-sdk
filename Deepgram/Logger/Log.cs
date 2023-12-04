namespace Deepgram.Logger;
internal static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "`{requestType}` to `{endpoint}` threw HttpRequestException  ")]
    internal static partial void HttpRequestException(this ILogger logger, string requestType, string endpoint, Exception ex);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "`{requestType}` threw `{exceptionType}` ")]
    internal static partial void Exception(this ILogger logger, string requestType, string exceptionType, Exception ex);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Whilst `{operationType}` `{objectType}` threw {exceptionType} ")]
    internal static partial void SerializerException(this ILogger logger, string operationType, string exceptionType, string objectType, Exception ex);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Error occurred whilst get values for query parameter class type of `{classType}`")]
    internal static partial void ParameterStringError(this ILogger logger, string classType, Exception ex);



    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Whilst creating `{clientType}` there was no ApiKey ")]
    internal static partial void ApiKeyNotPresent(this ILogger logger, string clientType);

}
