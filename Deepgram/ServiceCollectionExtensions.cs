using Microsoft.Extensions.DependencyInjection;

namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services)
    {
        services.AddHttpClient(Constants.HTTPCLIENT_NAME);
        return services;
    }
}
