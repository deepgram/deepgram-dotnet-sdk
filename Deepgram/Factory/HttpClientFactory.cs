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
        if (string.IsNullOrEmpty(httpId))
            httpId = HTTPCLIENT_NAME;

        Log.Information("HttpClientFactory.Create", $"HttpClient ID: {httpId}");

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

    internal static HttpClient ConfigureDeepgram(HttpClient client, IDeepgramClientOptions? options = null)
    {
        options ??= new DeepgramHttpClientOptions();

        // headers
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());

        // Set authentication header with priority: AccessToken > ApiKey
        SetAuthenticationHeader(client, options);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        client.BaseAddress = new Uri(options.BaseAddress);

        return client;
    }

    /// <summary>
    /// Sets the appropriate authentication header based on available credentials.
    /// Priority: AccessToken (Bearer) > ApiKey (Token)
    /// </summary>
    /// <param name="client">HttpClient to configure</param>
    /// <param name="options">Client options containing credentials</param>
    private static void SetAuthenticationHeader(HttpClient client, IDeepgramClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            // Use Bearer token authentication (highest priority)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
        }
        else if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            // Use API key authentication (fallback)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", options.ApiKey);
        }
        else
        {
            // No authentication credentials available
            Log.Warning("SetAuthenticationHeader", "No authentication credentials provided. API calls may fail.");
        }
    }
}
