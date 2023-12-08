namespace Deepgram.Extensions;
public static class ClientWebSocketExtensions
{
    public static ClientWebSocket SetHeaders(this ClientWebSocket clientWebSocket, DeepgramClientOptions options)
    {
        clientWebSocket.Options.SetRequestHeader("Authorization", $"token {options.ApiKey}");

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            { clientWebSocket.Options.SetRequestHeader(header.Key, header.Value); }

        return clientWebSocket;
    }
}
