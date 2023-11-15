using System.Text.RegularExpressions;
using Deepgram.Models.Options;

namespace Deepgram.Utilities;

internal static class HttpConfigureUtil
{
    internal static HttpClient Configure(string apiKey, DeepgramClientOptions options, HttpClient client)
    {
        client = SetBaseUrl(options, client);
        client = SetHeaders(apiKey, options, client);
        return client;
    }


    private static HttpClient SetBaseUrl(DeepgramClientOptions options, HttpClient httpClient)
    {
        //if the HttpClient BaseAddress is pre-set then return it 
        if (httpClient.BaseAddress != null) return httpClient;

        if (options.Url != null)
        {
            string pattern = @"^.*//(http://|https://)?";
            var url = Regex.Replace(options.Url, pattern, "").TrimEnd('/');
            httpClient.BaseAddress = new Uri($"https://{url}");
        }
        else
        {
            httpClient.BaseAddress = new($"https://{Constants.DEFAULT_URI}");
        }

        return httpClient;
    }

    private static HttpClient SetHeaders(string? apiKey, DeepgramClientOptions options, HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);


        if (options.Headers is not null)
            foreach (var header in options.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        return httpClient;
    }
}
