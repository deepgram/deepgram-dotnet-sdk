using Deepgram;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.AddHttpClient();
var clientSettings = new ClientSettings()
{
    BaseAddress = "https://some.com",
    ProxyAddress = "http://pox.com:8080",
    TimeoutInSeconds = 60,
    Headers = new Dictionary<string, string>()
    {
        {"oneKey","oneValue" },
    }

};
serviceCollection.AddDeepgram(clientSettings);
var serviceProvider = serviceCollection.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
var ApiKey = "6649c6cc06d174902ef8051f63f139f34bb87424";

var client = new ManageClient(ApiKey, httpClientFactory);


// Act
var result = await client.GetProjects();

var x = 1;