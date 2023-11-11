using Microsoft.Extensions.DependencyInjection;

namespace Deepgram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepgram(this IServiceCollection services)
    {
        services.AddHttpClient<PrerecordedClient>();


        services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
        {
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        });

        services.AddTransient<LiveClient, LiveClient>();
        services.AddTransient<ClientWebSocket, ClientWebSocket>();


        return services;
    }
}
