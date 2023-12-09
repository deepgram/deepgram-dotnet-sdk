using Deepgram.Records;
using Deepgram.Records.OnPrem;

namespace Deepgram.Tests.UnitTests.ClientTests;
public class OnPremClientTests
{
    DeepgramClientOptions _options;
    string _projectId;
    readonly string _urlPrefix = $"/{Constants.API_VERSION}/{Constants.PROJECTS}";
    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions("fake");
        _projectId = new Faker().Random.Guid().ToString();
    }


    [Test]
    public void OnPremClient_urlPrefix_Should_Match_Expected()
    {
        //Arrange 
        var expected = "/v1/projects";
        var client = new OnPremClient(_options, new HttpClient());


        //Assert
        using (new AssertionScope())
        {
            client.UrlPrefix.Should().NotBeNull();
            client.UrlPrefix.Should().BeEquivalentTo(expected);
        }
    }

    [Test]
    public async Task ListCredentials_Should_Call_GetAsync_Returning_ListOnPremCredentialsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<ListOnPremCredentialsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var onPremClient = Substitute.For<OnPremClient>(_options, httpClient);
        onPremClient.When(x => x.GetAsync<ListOnPremCredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<ListOnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}").Returns(expectedResponse);

        // Act

        var result = await onPremClient.ListCredentials(_projectId);

        // Assert
        await onPremClient.Received().GetAsync<ListOnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}");
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
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var credentialsId = new Faker().Random.Guid().ToString();
        var onPremClient = Substitute.For<OnPremClient>(_options, httpClient);
        onPremClient.When(x => x.GetAsync<OnPremCredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<OnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}/{credentialsId}").Returns(expectedResponse);

        // Act
        var result = await onPremClient.GetCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().GetAsync<OnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}/{credentialsId}");
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
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var credentialsId = new Faker().Random.Guid().ToString();
        var onPremClient = Substitute.For<OnPremClient>(_options, httpClient);
        onPremClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.DeleteAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}/{credentialsId}").Returns(expectedResponse);

        // Act

        var result = await onPremClient.DeleteCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().DeleteAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}/{credentialsId}");

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
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var client = new OnPremClient(_options, httpClient);
        var createOnPremCredentialsSchema = new CreateOnPremCredentialsSchema();

        var onPremClient = Substitute.For<OnPremClient>(_options, httpClient);
        onPremClient.When(x => x.PostAsync<OnPremCredentialsResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        onPremClient.PostAsync<OnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act

        var result = await onPremClient.CreateCredentials(_projectId, createOnPremCredentialsSchema);

        // Assert
        await onPremClient.Received().PostAsync<OnPremCredentialsResponse>($"{_urlPrefix}/{_projectId}/{Constants.ONPREM}", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<OnPremCredentialsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
