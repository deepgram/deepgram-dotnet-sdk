using Deepgram.Extensions;
using Deepgram.Models.Shared.v1;

namespace Deepgram.DeepgramHttpClient;
internal class DeepgramHttpClientFactory
{
    public static HttpClientWrapper Create(string apiKey, DeepgramClientOptions deepgramClientOptions)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(Defaults.HTTPCLIENT_NAME)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));
        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME);
        client.ConfigureDeepgram(apiKey, deepgramClientOptions);
        client.Timeout = TimeSpan.FromMinutes(5);
        return new HttpClientWrapper(client);
    }
}
