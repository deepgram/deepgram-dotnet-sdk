namespace Deepgram.DeepgramHttpClient;

internal class HttpClientWrapper(HttpClient HttpClient)
{
    internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return HttpClient.SendAsync(request);
    }

    /// <summary>
    /// this needs to be able to be set inbetween calls, as
    /// the longer the recording and more options selected the longer the transcription takes
    /// and the default timeout can be quickly exceeded
    /// </summary>
    internal void SetTimeOut(TimeSpan timeSpan)
    {
        HttpClient.Timeout = timeSpan;
    }
}
