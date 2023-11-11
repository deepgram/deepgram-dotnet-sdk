namespace Deepgram.Utilities;

internal static class HttpConfigureUtil
{
    internal static HttpClient SetBaseUrl(string url, HttpClient httpClient)
    {
        httpClient.BaseAddress = new($"https://{url}");
        return httpClient;
    }

    internal static HttpClient SetHeaders(string? apiKey, Dictionary<string, string>? headers, HttpClient httpClient)
    {

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);


        if (headers is not null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
        return httpClient;
    }
}
