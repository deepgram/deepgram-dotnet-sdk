namespace Deepgram.Extensions;

internal static class HttpClientExtensions
{
    internal static HttpClient ConfigureDeepgram(this HttpClient client, DeepgramClientOptions options)
    {
        SetBaseUrl(client, options);
        client.SetDefaultHeaders(options);
        return client;
    }

    private static void SetDefaultHeaders(this HttpClient client, DeepgramClientOptions options)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", options.ApiKey);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            { client.DefaultRequestHeaders.Add(header.Key, header.Value); }
    }

    internal static void SetBaseUrl(HttpClient client, DeepgramClientOptions deepgramClientOptions)
    {

        var baseAddress = deepgramClientOptions.BaseAddress;
        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        Regex regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
            baseAddress = regex.Replace(baseAddress, "https://");
        else
            //if no protocol in the address then https:// is added
            baseAddress = $"https://{baseAddress}";

        client.BaseAddress = new Uri(baseAddress);
    }
}

