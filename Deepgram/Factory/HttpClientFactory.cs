// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Encapsulations;

internal class HttpClientFactory
{
    public const int DEFAULT_HTTP_TINEOUT_IN_MINUTES = 5;
    public const string HTTPCLIENT_NAME = "DEEPGRAM_HTTP_CLIENT";

    public static HttpClient Create(string? httpId = null)
    {
        if (string.IsNullOrWhiteSpace(httpId))
            httpId = HTTPCLIENT_NAME;

        var services = new ServiceCollection();
        services.AddHttpClient(httpId ?? HTTPCLIENT_NAME)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));
        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpId);

        // this is ok because we are using CancellationTokenSource with a default DefaultRESTTimeout timeout
        client.Timeout = Timeout.InfiniteTimeSpan;

        return client;
    }

    internal static HttpClient ConfigureDeepgram(HttpClient client, DeepgramHttpClientOptions? options = null)
    {
        options ??= new DeepgramHttpClientOptions();

        // headers
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", options.ApiKey);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        client.BaseAddress = new Uri(options.BaseAddress);

        return client;
    }
}
