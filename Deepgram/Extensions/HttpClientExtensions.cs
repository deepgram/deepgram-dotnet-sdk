// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Extensions;

internal static class HttpClientExtensions
{
    internal static HttpClient ConfigureDeepgram(this HttpClient client, string apiKey, DeepgramClientOptions options)
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
    private static void SetDefaultHeaders(this HttpClient client, string apiKey, DeepgramClientOptions options)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            { client.DefaultRequestHeaders.Add(header.Key, header.Value); }
    }

    internal static void SetBaseUrl(this HttpClient client, DeepgramClientOptions deepgramClientOptions)
    {

        var baseAddress = $"{deepgramClientOptions.BaseAddress}/{deepgramClientOptions.APIVersion}/";

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

