namespace Deepgram.Extensions;
public static class ClientWebSocketExtensions
{
    public static ClientWebSocket SetHeaders(this ClientWebSocket clientWebSocket, string apiKey, DeepgramClientOptions? options)
    {
        clientWebSocket.Options.SetRequestHeader("Authorization", $"token {apiKey}");

        if (options is not null && options.Headers is not null)
            foreach (var header in options.Headers)
            { clientWebSocket.Options.SetRequestHeader(header.Key, header.Value); }

        return clientWebSocket;
    }
}
