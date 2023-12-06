namespace Deepgram.Logger;
internal static partial class Log
{
    [LoggerMessage(
        EventId = 0001,
        Level = LogLevel.Error,
        Message = "`{requestType}` to `{endpoint}` threw HttpRequestException  ")]
    internal static partial void HttpRequestException(this ILogger logger, string requestType, string endpoint, Exception ex);

    [LoggerMessage(
        EventId = 0002,
        Level = LogLevel.Error,
        Message = "`{requestType}` threw `{exceptionType}` ")]
    internal static partial void Exception(this ILogger logger, string requestType, string exceptionType, Exception ex);

    [LoggerMessage(
        EventId = 0004,
        Level = LogLevel.Error,
        Message = "Error occurred whilst get values for query parameter class type of `{classType}`")]
    internal static partial void ParameterStringError(this ILogger logger, string classType, Exception ex);


    [LoggerMessage(
        EventId = 0005,
        Level = LogLevel.Information,
        Message = "Whilst creating `{clientType}` there was no ApiKey ")]
    internal static partial void ApiKeyNotPresent(this ILogger logger, string clientType);


    [LoggerMessage(
        EventId = 0006,
        Level = LogLevel.Warning,
        Message = "Trying to send message when the socket is `{state}`. Ack for this message will fail shortly.")]
    internal static partial void LiveSendWarning(this ILogger logger, WebSocketState state);

    [LoggerMessage(
        EventId = 0007,
        Level = LogLevel.Error,
        Message = "Failed to enqueue message to WebSocket connection.")]
    internal static partial void EnqueueFailure(this ILogger logger);

    [LoggerMessage(
       EventId = 0009,
       Level = LogLevel.Error,
       Message = "Unable to perform `{action}` Socket disposed")]
    internal static partial void SocketDisposed(this ILogger logger, string action, Exception ex);


    [LoggerMessage(
        EventId = 0010,
        Level = LogLevel.Error,
        Message = "WebSocket Receipt Error")]
    internal static partial void ReceiptError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 0011,
        Level = LogLevel.Error,
        Message = "Error Sending to WebSocket")]
    internal static partial void SendWebSocketError(this ILogger logger, Exception ex);

    [LoggerMessage(
       EventId = 0012,
       Level = LogLevel.Error,
       Message = "WebSocket send operation cancelled")]
    internal static partial void SendCancelledError(this ILogger logger, Exception ex);

    [LoggerMessage(
       EventId = 0013,
       Level = LogLevel.Error,
       Message = "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.")]
    internal static partial void SocketStartError(this ILogger logger, Exception ex);

    [LoggerMessage(
      EventId = 0014,
      Level = LogLevel.Error,
      Message = "Closing WebSocket")]
    internal static partial void ClosingSocket(this ILogger logger);

    [LoggerMessage(
      EventId = 0015,
      Level = LogLevel.Error,
      Message = "Staring Connection Async")]
    internal static partial void WebSocketStartError(this ILogger logger, Exception ex);

    [LoggerMessage(
      EventId = 0016,
      Level = LogLevel.Information,
      Message = "WebSocket is being closed : `{closeStatusDescription}`")]
    internal static partial void RequestedSocketClose(this ILogger logger, string closeStatusDescription);

    [LoggerMessage(
     EventId = 0017,
     Level = LogLevel.Error,
     Message = "Error closing WebSocket")]
    internal static partial void WebSocketCloseError(this ILogger logger, Exception ex);


    [LoggerMessage(
    EventId = 0018,
    Level = LogLevel.Error,
    Message = "Error creating project key both ExpirationDate and TimeToLiveInSeconds are set: `{createProjectKeySchema}`")]
    internal static partial void CreateProjectKeyError(this ILogger logger, CreateProjectKeySchema createProjectKeySchema);
}
