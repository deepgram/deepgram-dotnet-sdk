using Microsoft.Extensions.DependencyInjection;

namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services)
    {
        //services.AddHttpClient(Constants.HTTPCLIENT_NAME, client =>
        //{
        //    client.BaseAddress = new($"https://{Constants.DEFAULT_URI}/{Constants.API_VERSION}");
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetInfo());
        //});

        services.AddHttpClient();

        services.AddTransient<LiveClient, LiveClient>();
        services.AddTransient<ClientWebSocket, ClientWebSocket>();


        return services;
    }
}
