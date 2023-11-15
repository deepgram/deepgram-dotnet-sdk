using Deepgram.Models;

namespace Deepgram.Tests.ClientTests;
public class EndpointTests
{
    public string ApiKey = "";


    [Test]
    public async Task GetAsync_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange        
        var excpectedResult = new AutoFaker<GetProjectsResponse>().Generate();
        var deepgramResponse = new DeepgramResponse<GetProjectsResponse>()
        {
            Error = null,
            Result = excpectedResult
        };
        var uriSegment = Constants.PROJECTS_URI_SEGMENT;
        // var httpClient = MockHttpClient.CreateHttpClientWithResult(deepgramResponse, HttpStatusCode.OK);
        var httpClient = new HttpClient();

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = Substitute.For<ILogger<ConcreteRestClient>>();

        // Act
        var result = await client.GetAsync<GetProjectsResponse>(uriSegment);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<GetProjectsResponse>(result);
        // Assert.That(result.Projects.Length, Is.EqualTo(excpectedResult.Projects.Length));
    }

    [Test]
    public async Task PostTest()
    {
        var source = new UrlSource { Url = "https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav" };
        var options = new PrerecordedSchema { Punctuate = true, Utterances = true };
        var ops = QueryParameterUtil.GetParameters(options);
        var uriSegment = $"{Constants.API_VERSION}/{Constants.TRANSCRIPTION_URI_SEGMENT}?{ops}";

        var httpClient = new HttpClient();
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = Substitute.For<ILogger<ConcreteRestClient>>();

        // Act
        var result = await client.PostAsync<SyncPrerecordedResponse, UrlSource>(uriSegment, source);


        Assert.That(result, Is.Not.Null);
    }



}
