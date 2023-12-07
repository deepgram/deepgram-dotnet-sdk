namespace Deepgram;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Deepgram services to the service collection
    /// </summary>
    /// <param name="options">The options to use with the registered deepgram services</param>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, DeepgramClientOptions options)
    {
        // Register the http client
        services.AddHttpClient(Constants.HTTPCLIENT_NAME)
    .AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

        // Should we use this?
        services.AddHttpClient(Constants.WSSCLIENT_NAME)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

        services.AddTransient(sp =>
        {
            var httpClient = sp.GetService<IHttpClientFactory>().CreateClient(Constants.HTTPCLIENT_NAME);
            return new PrerecordedClient(options, httpClient);
        });

        services.AddTransient(sp =>
        {
            var httpClient = sp.GetService<IHttpClientFactory>().CreateClient(Constants.HTTPCLIENT_NAME);
            return new ManageClient(options, httpClient);
        });

        // TODO: Add the onprem client

        services.AddTransient(_ => new LiveClient(options));

        return services;
    }

    /// <summary>
    /// Adds the Deepgram services to the service collection with the default options
    /// </summary>
    /// <param name="apiKey">The API key to use to authenticate with Deepgram</param>
    /// <returns></returns>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, string apiKey)
    {
        var deepgramOptions = new DeepgramClientOptions(apiKey)
        { 
        };
        return services.AddDeepgram(deepgramOptions);
    }
}
