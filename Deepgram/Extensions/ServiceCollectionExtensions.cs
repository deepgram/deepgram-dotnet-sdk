namespace Deepgram.Extensions;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Adds the Deepgram services to the service collection with the default options
    /// </summary>
    /// <param name="apiKey">The API key to use to authenticate with Deepgram</param>
    /// <returns></returns>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, string apiKey)
    {
        return services.AddDeepgram(new DeepgramClientOptions(apiKey));
    }

    /// <summary>
    /// Adds the Deepgram services to the service collection
    /// </summary>
    /// <param name="options">The options to use with the registered deepgram services</param>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, DeepgramClientOptions? options = null)
    {
        // Register the http client
        services.AddHttpClient(Defaults.HTTPCLIENT_NAME,
            (hc) => hc.Timeout = (TimeSpan)options.HttpTimeout)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));


        services.AddTransient(sp =>
        {
            return new PrerecordedClient(options,
                sp.GetService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME));
        });

        services.AddTransient(sp =>
        {
            return new ManageClient(options,
                sp.GetService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME));
        });

        services.AddTransient(sp =>
        {
            return new OnPremClient(options,
                sp.GetService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME));
        });

        services.AddTransient(_ => new LiveClient(options));
        return services;
    }


}
