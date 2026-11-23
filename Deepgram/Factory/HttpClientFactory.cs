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
    ///
    /// The value is a Lazy so the provider is built exactly once per ID. ConcurrentDictionary
    /// may run its value factory on more than one thread and discard the losers, which would
    /// otherwise construct and silently drop an IDisposable provider.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<IServiceProvider>> _providers =
        new ConcurrentDictionary<string, Lazy<IServiceProvider>>();

    private static IServiceProvider GetProvider(string httpId) =>
        _providers.GetOrAdd(httpId, static id => new Lazy<IServiceProvider>(() =>
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
            var httpBuilder = services.AddHttpClient(id);

            // Connection reuse comes from caching the provider above and applies everywhere.
            // Bounding the connection lifetime is an additional benefit that only makes sense
            // where SocketsHttpHandler is the right primary handler: it is unsupported on
            // browser (Blazor WebAssembly throws PlatformNotSupportedException), and on Android
            // and iOS it would bypass the platform's native HTTP stack, changing TLS, proxy and
            // client-certificate behaviour. Leave those platforms on the default handler.
            if (!OperatingSystem.IsBrowser() && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
            {
                httpBuilder.ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = HttpClientFactory.PooledConnectionLifetime,
                });

                // PooledConnectionLifetime already rotates connections, so the factory's own
                // handler rotation would churn them a second time.
                httpBuilder.SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            }

            httpBuilder.AddResilienceHandler("deepgram-retry", builder =>
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
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

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

        // Validate authentication now; REST clients apply it to each request so credential rotation
        // and request-specific Authorization headers work without shared-header mutation.
        ValidateAuthentication(options);

        if (options.Headers is not null)
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        client.BaseAddress = new Uri(options.BaseAddress);

        return client;
    }

    /// <summary>
    /// Validates that the client has credentials when its endpoint requires them.
    /// </summary>
    /// <param name="options">Client options containing credentials</param>
    private static void ValidateAuthentication(IDeepgramClientOptions options)
    {
        var header = GetAuthenticationHeader(options);
        if (header != null)
        {
            return;
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

    /// <summary>
    /// Builds the Authorization header for the credentials currently held by the options (or null if none).
    /// Used per request so rotated credentials apply without mutating the shared HttpClient.
    /// </summary>
    internal static AuthenticationHeaderValue? GetAuthenticationHeader(IDeepgramClientOptions options)
    {
        if (options is IAuthenticationHeaderOptions authenticationHeaderOptions)
        {
            return authenticationHeaderOptions.GetAuthenticationHeader();
        }

        // Custom IDeepgramClientOptions implementations own their synchronization.
        var accessToken = options.AccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }
        var apiKey = options.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return new AuthenticationHeaderValue("token", apiKey);
        }
        return null;
    }
}
