// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Encapsulations;

internal class HttpClientFactory
{
    public const int DEFAULT_HTTP_TINEOUT_IN_MINUTES = 5;
    public const string HTTPCLIENT_NAME = "DEEPGRAM_HTTP_CLIENT";

    public static HttpClient Create(string httpId = HTTPCLIENT_NAME)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(httpId)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));
        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpId);

        // this is ok because we are using CancellationTokenSource with a default DefaultRESTTimeout timeout
        client.Timeout = Timeout.InfiniteTimeSpan;

        return client;
    }

    internal static HttpClient ConfigureDeepgram(HttpClient client, string apiKey = "", DeepgramClientOptions? options = null)
    {
        options ??= new DeepgramClientOptions();

        // user provided takes precedence
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // then try the environment variable
            // TODO: log
            apiKey = Environment.GetEnvironmentVariable(variable: Defaults.DEEPGRAM_API_KEY) ?? "";
            if (string.IsNullOrEmpty(apiKey))
            {
                // TODO: log
            }
        }
        if (!options.OnPrem && string.IsNullOrEmpty(apiKey))
        {
            // TODO: log
            throw new ArgumentException("Deepgram API Key is invalid");
        }

        // headers
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        // base url
        var baseAddress = $"{Defaults.DEFAULT_URI}/{options.APIVersion}/";
        if (options.BaseAddress is not null)
        {
            // TODO: log
            baseAddress = $"{options.BaseAddress}/{options.APIVersion}/";
        }
        // TODO: log

        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        Regex regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(baseAddress))
        {
            //if no protocol in the address then https:// is added
            // TODO: log
            baseAddress = $"https://{baseAddress}";
        }

        // TODO: log
        client.BaseAddress = new Uri(baseAddress);

        return client;
    }
}
