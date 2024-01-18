namespace Deepgram.Extensions;

internal static class HttpClientExtensions
{
    internal static HttpClient ConfigureDeepgram(this HttpClient client, string apiKey, DeepgramClientOptions? options)
    {
        ValidateApiKey(apiKey);
        client.SetDefaultHeaders(apiKey, options);
        client.SetBaseUrl(options);
        return client;
    }
    private static void ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Log.ApiKeyNotPresent(LogProvider.GetLogger(nameof(HttpClientExtensions)));
            throw new ArgumentNullException(nameof(apiKey));
        }

    }
    private static void SetDefaultHeaders(this HttpClient client, string apiKey, DeepgramClientOptions? options)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);

        if (options is not null && options.Headers is not null)
            foreach (var header in options.Headers)
            { client.DefaultRequestHeaders.Add(header.Key, header.Value); }
    }

    internal static void SetBaseUrl(this HttpClient client, DeepgramClientOptions? deepgramClientOptions)
    {

        var baseAddress = string.Empty;
        if (deepgramClientOptions is not null && deepgramClientOptions.BaseAddress is not null)
        {
            baseAddress = deepgramClientOptions.BaseAddress;
        }
        else
        {
            baseAddress = Defaults.DEFAULT_URI;
        }


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

