// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Models.Authenticate.v1;
#if NETSTANDARD2_0
using Polly.Contrib.WaitAndRetry;
#else
using Microsoft.Extensions.Http.Resilience;
#endif

namespace Deepgram.Encapsulations;

internal class HttpClientFactory
{
    public const int DEFAULT_HTTP_TINEOUT_IN_MINUTES = 5;
    public const string HTTPCLIENT_NAME = "DEEPGRAM_HTTP_CLIENT";

    /// <summary>
    /// How long a pooled TCP connection may be reused before it is replaced. Bounding the
    /// lifetime lets a long-lived client pick up DNS changes; see #498.
    /// </summary>
    internal static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// One service provider per HTTP client ID, built once and kept for the life of the process.
    ///
    /// IHttpClientFactory pools the HttpMessageHandler (and therefore the TCP connection pool)
    /// per client name *inside a single provider*. Building a provider per client - which this
    /// factory used to do - gave every Deepgram client its own handler and its own connection
    /// pool, so no connection was ever reused between them. Caching the provider is what makes
    /// connection reuse possible; the client name remains a real isolation boundary.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IServiceProvider> _providers =
        new ConcurrentDictionary<string, IServiceProvider>();

    private static IServiceProvider GetProvider(string httpId) =>
        _providers.GetOrAdd(httpId, static id =>
        {
            var services = new ServiceCollection();
#if NETSTANDARD2_0
            // Microsoft.Extensions.Http.Resilience does not support netstandard2.0, so this target
            // keeps the Polly-based transient-error retry. See #359.
            services.AddHttpClient(id)
                .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));
#else
            // Retry transient HTTP errors (5xx, 408, HttpRequestException) with exponential backoff +
            // jitter. This is the Polly v8 equivalent of the previous AddTransientHttpErrorPolicy +
            // DecorrelatedJitterBackoffV2(1s, 5) policy, now that Polly.Extensions.Http is deprecated.
            // See #359.
            services.AddHttpClient(id)
                .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = HttpClientFactory.PooledConnectionLifetime,
                })
                .AddResilienceHandler("deepgram-retry", builder =>
                {
                    builder.AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 5,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(1),
                    });
                });
#endif
            return services.BuildServiceProvider();
        });

    public static HttpClient Create(string? httpId = null)
    {
        if (string.IsNullOrEmpty(httpId))
            httpId = HTTPCLIENT_NAME;

        Log.Information("HttpClientFactory.Create", $"HttpClient ID: {httpId}");

        // Clients sharing an ID share the pooled handler, and therefore the TCP connections.
        var client = GetProvider(httpId!).GetRequiredService<IHttpClientFactory>().CreateClient(httpId!);

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
        else if (options is DeepgramHttpClientOptions { AllowAnonymous: true })
        {
            // Public endpoint: send the request without an Authorization header.
            Log.Debug("SetAuthenticationHeader", "Public endpoint; no Authorization header sent.");
        }
        else
        {
            // No authentication credentials available
            Log.Warning("SetAuthenticationHeader", "No authentication credentials provided. API calls may fail.");
        }
    }
}
