using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services, DeepgramClientOptions? clientSettings = null)
    {
        services.AddHttpClient(Constants.HTTPCLIENT_NAME)
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                if (clientSettings is null)
                {
                    httpClient.BaseAddress = new Uri(Constants.DEFAULT_URI);
                }
                else
                {
                    httpClient.BaseAddress = clientSettings.BaseAddress is not null
                    ? new Uri(clientSettings.BaseAddress)
                    : new Uri(Constants.DEFAULT_URI);

                    if (clientSettings.TimeoutInSeconds is not null)
                        httpClient.Timeout = TimeSpan.FromSeconds((int)clientSettings.TimeoutInSeconds!);


                    if (clientSettings.Headers is not null)
                        foreach (var header in clientSettings.Headers)
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }



                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());



            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = SetProxy(clientSettings)
            });

        return services;
    }

    private static IWebProxy? SetProxy(DeepgramClientOptions? clientSettings)
    {
        if (clientSettings is not null && clientSettings.Proxy is not null)
        {
            var proxy = new WebProxy(clientSettings.Proxy.ProxyAddress);
            if (clientSettings.Proxy.Password is not null && clientSettings.Proxy.Username is not null)
                proxy.Credentials = new NetworkCredential(
                    clientSettings.Proxy.Username,
                    clientSettings.Proxy.Password);

            return proxy;

        }
        return null;
    }

}
