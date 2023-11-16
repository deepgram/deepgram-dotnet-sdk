using Deepgram.Models;

namespace Deepgram.Tests.ClientTests;

[TestFixture]
public class AbstractRestfulClientTests
{
    ILogger<ConcreteRestClient> logger;
    string ApiKey;

    [SetUp]
    public void Setup()
    {
        logger = Substitute.For<ILogger<ConcreteRestClient>>();
        ApiKey = new Faker().Random.Guid().ToString();
    }

    [Test]
    public async Task GetAsync_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange        
        var expectedResult = new AutoFaker<GetProjectsResponse>().Generate();
        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResult, HttpStatusCode.OK);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;

        // Act
        var result = await client.GetAsync<GetProjectsResponse>(uriSegment);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<GetProjectsResponse>(result);
        Assert.That(result.Projects.Length, Is.EqualTo(expectedResult.Projects.Length));
    }

    [Test]
    public void GetAsync_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<GetProjectsResponse>().Generate();
        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync<GetProjectsResponse>(uriSegment));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }

    [Test]
    public async Task GetAsync_With_Id_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange    
        var id = new Faker().Random.Guid().ToString();
        var expectedResponse = new AutoFaker<GetProjectResponse>().Generate();
        expectedResponse.ProjectId = id;

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        var result = await client.GetAsync<GetProjectResponse>(uriSegment);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<GetProjectResponse>(result);
        Assert.That(result.ProjectId, Is.EqualTo(expectedResponse.ProjectId));
    }

    [Test]
    public void GetAsync_With_Id_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new GetProjectsResponse();
        var id = new Faker().Random.Guid().ToString();
        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}/{id}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act & Assert       
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync<GetProjectsResponse>(uriSegment));
        logger.Received().AnyLogOfType(LogLevel.Error, "Error occurred during GET request");
    }

    [Test]
    public async Task PostAsync_Should_Return_Response_Model_With_Schema_And_No_Body()
    {
        // Arrange

        var id = new Faker().Random.Guid().ToString();
        var expectedResponse = new AutoFaker<CreateProjectKeyResponse>().Generate();

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        var result = await client.PostAsync<CreateProjectKeyResponse, object>(uriSegment, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<CreateProjectKeyResponse>(result);
        Assert.That(result.ApiKeyId, Is.EqualTo(expectedResponse.ApiKeyId));
    }

    [Test]
    public async Task PostAsync_Should_Return_Response_Model_With_Schema_And_Body()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<SyncPrerecordedResponse>().Generate();
        var source = new UrlSource { Url = "https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav" };

        var schema = new PrerecordedSchema { Punctuate = true, Utterances = true };
        var stringedSchema = QueryParameterUtil.GetParameters(schema);

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}?{stringedSchema}";

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        var result = await client.PostAsync<SyncPrerecordedResponse, UrlSource>(uriSegment, source);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<SyncPrerecordedResponse>(result);
    }

    [Test]
    public Task PostAsync_Should_Throw_Exception_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<SyncPrerecordedResponse>().Generate();
        var source = new UrlSource { Url = "https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav" };

        var schema = new PrerecordedSchema { Punctuate = true, Utterances = true };
        var stringedSchema = QueryParameterUtil.GetParameters(schema);

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}?{stringedSchema}";

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        // Act & Assert       
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync<SyncPrerecordedResponse, UrlSource>(uriSegment, source));
        logger.Received().AnyLogOfType(LogLevel.Error, "Error occurred during POST request");
        return Task.CompletedTask;
    }


    [Test]
    public void DeleteAsync_Should_Return_Nothing_On_SuccessfulResponse()
    {
        // Arrange        
        var expectedResult = new VoidResponse();
        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResult, HttpStatusCode.OK);
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        AsyncTestDelegate act = async () => await client.DeleteAsync(uriSegment);

        // Assert
        Assert.DoesNotThrowAsync(act);
    }


    [Test]
    public void DeleteAsync_Should_ThrowsException_On_Response_containing_Error()
    {
        // Arrange       
        var expectedResult = new VoidResponse()
        {
            Error = new Exception()
        };

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResult, HttpStatusCode.BadRequest);
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.DeleteAsync(uriSegment));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }

    [Test]
    public async Task DeleteAsync_TResponse_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange        
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        // Act
        var result = await client.DeleteAsync<MessageResponse>(uriSegment);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.That(result.Message, Is.EqualTo(expectedResponse.Message));
    }

    [Test]
    public void DeleteAsync_TResponse_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var uriSegment = $"{Constants.PROJECTS_URI_SEGMENT}";


        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClient);
        client.Logger = logger;
        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.DeleteAsync(uriSegment));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }

}
