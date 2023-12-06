namespace Deepgram.Utilities;
public static class WssClientUtil
{
    public static ClientWebSocket SetHeaders(string apiKey, DeepgramClientOptions options, ClientWebSocket clientWebSocket)
    {
        clientWebSocket.Options.SetRequestHeader("Authorization", $"token {apiKey}");

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            { clientWebSocket.Options.SetRequestHeader(header.Key, header.Value); }

        return clientWebSocket;
    }
}
