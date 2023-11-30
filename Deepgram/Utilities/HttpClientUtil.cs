namespace Deepgram.Utilities;

internal static class HttpClientUtil
{
    internal static HttpClient Configure(string apiKey, DeepgramClientOptions options, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME);
        client.BaseAddress = new Uri(options.BaseAddress);
        client = SetDefaultHeaders(apiKey, options, client);
        return client;
    }

    private static HttpClient SetDefaultHeaders(string? apiKey, DeepgramClientOptions options, HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            { httpClient.DefaultRequestHeaders.Add(header.Key, header.Value); }

        return httpClient;
    }
}

