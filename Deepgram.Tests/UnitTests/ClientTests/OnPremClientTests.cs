using Deepgram.DeepgramHttpClient;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.OnPrem.v1;
using Deepgram.Models.Shared.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;
public class OnPremClientTests
{
    DeepgramClientOptions _options;
    string _projectId;
    string _apiKey;
    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions();
        _projectId = new Faker().Random.Guid().ToString();
        _apiKey = new Faker().Random.Guid().ToString();
    }




    [Test]
    public async Task ListCredentials_Should_Call_GetAsync_Returning_ListOnPremCredentialsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<ListOnPremCredentialsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        onPremClient._httpClientWrapper = new HttpClientWrapper(httpClient);

        onPremClient.When(x => x.GetAsync<ListOnPremCredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<ListOnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}").Returns(expectedResponse);

        // Act

        var result = await onPremClient.ListCredentials(_projectId);

        // Assert
        await onPremClient.Received().GetAsync<ListOnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ListOnPremCredentialsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetCredentials_Should_Call_GetAsync_Returning_OnPremCredentialsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<OnPremCredentialsResponse>().Generate();
        var credentialsId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        onPremClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        onPremClient.When(x => x.GetAsync<OnPremCredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<OnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}").Returns(expectedResponse);

        // Act
        var result = await onPremClient.GetCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().GetAsync<OnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<OnPremCredentialsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteCredentials_Should_Call_DeleteAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var credentialsId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        onPremClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        onPremClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}").Returns(expectedResponse);

        // Act

        var result = await onPremClient.DeleteCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }


    [Test]
    public async Task CreateCredentials_Should_Return_OnPremCredentialsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<OnPremCredentialsResponse>().Generate();
        var createOnPremCredentialsSchema = new CreateOnPremCredentialsSchema();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        onPremClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        onPremClient.When(x => x.PostAsync<OnPremCredentialsResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        onPremClient.PostAsync<OnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act

        var result = await onPremClient.CreateCredentials(_projectId, createOnPremCredentialsSchema);

        // Assert
        await onPremClient.Received().PostAsync<OnPremCredentialsResponse>($"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<OnPremCredentialsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
