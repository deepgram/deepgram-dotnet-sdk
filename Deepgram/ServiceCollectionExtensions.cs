using Microsoft.Extensions.DependencyInjection;

namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services, ClientConfigOptions? clientConfigOptions = null)
    {
        services.AddHttpClient(Constants.HTTPCLIENT_NAME)
            .ConfigurePrimaryHttpMessageHandler(() => ConfigureHandler(clientConfigOptions));

        return services;
    }

    private static HttpClientHandler ConfigureHandler(ClientConfigOptions? clientConfigOptions)
    {
        var handler = new HttpClientHandler();
        if (clientConfigOptions is not null && clientConfigOptions.Proxy is not null)
        {
            var webProxy = clientConfigOptions.Proxy;
            webProxy.BypassProxyOnLocal = false;
            webProxy.UseDefaultCredentials = false;
            handler.Proxy = webProxy;
            handler.UseProxy = true;
        }

        return handler;
    }
}
