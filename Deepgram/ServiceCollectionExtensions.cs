namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services)
    {
        services.AddTransient<LiveClient>();
        services.AddTransient<PrerecordedClient>();
        services.AddTransient<LiveClient>();

        services.AddTransient<ManageClient>();

        services.AddHttpClient(Constants.HTTPCLIENT_NAME)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

        return services;
    }
}
