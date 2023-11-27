namespace Deepgram.Tests.UnitTests;
public class ServiceCollectionTests
{

    //[Test]
    //public void AddDeepgram_Should_Set_HttpClient_With_Default_Headers_Url()
    //{
    //    //Arrange
    //    var services = new ServiceCollection();
    //    var options = new ClientConfigOptions()
    //    {
    //        Proxy = new RestProxy()
    //        {
    //            ProxyAddress = "https://acme.tnt",
    //            Username = "wiley",
    //            Password = "coyote"
    //        }
    //    };
    //    services.AddDeepgram(options);
    //    var serviceProvider = services.BuildServiceProvider();
    //    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    //    var ApiKey = new Faker().Random.Guid().ToString();

    //    //Act
    //    var client = new ConcreteRestClient(ApiKey, httpClientFactory);

    //    Assert.That(client, Is.Not.Null);
    //    Assert.That(client.HttpClient, Is.Not.Null);
    //    Assert.That(client.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
    //    Assert.That(client.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.Not.Null);
    //    Assert.That(client.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(ApiKey));

    //    Assert.That(client.HttpClient.DefaultRequestHeaders.UserAgent, Is.Not.Null);

    //    Assert.That(client.HttpClient.BaseAddress, Is.Not.Null);
    //    Assert.That(client.HttpClient.BaseAddress.ToString(), Is.EqualTo($"{Constants.DEFAULT_URI}/"));
    //}




}