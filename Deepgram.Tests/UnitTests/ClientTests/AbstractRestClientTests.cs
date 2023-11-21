using Deepgram.Models;

namespace Deepgram.Tests.UnitTests.ClientTests;

[TestFixture]
public class AbstractRestfulClientTests
{
    ILogger<ConcreteRestClient> logger;
    string ApiKey;
    IHttpClientFactory httpClientFactory;

    [SetUp]
    public void Setup()
    {
        logger = Substitute.For<ILogger<ConcreteRestClient>>();
        ApiKey = new Faker().Random.Guid().ToString();
        httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

    }

    [Test]
    public async Task GetAsync_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange        
        var expectedResponse = new AutoFaker<GetProjectsResponse>().Generate();
        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        client.Logger = logger;

        // Act
        var result = await client.GetAsync<GetProjectsResponse>(uriSegment);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<GetProjectsResponse>(result);
        Assert.That(result.Projects.Length, Is.EqualTo(expectedResponse.Projects.Length));
    }

    [Test]
    public void GetAsync_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange       
        var expectedResponse = new AutoFaker<GetProjectsResponse>().Generate();
        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
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

        var uriSegment = $"{Constants.PROJECTS}";


        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
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
        var uriSegment = $"{Constants.PROJECTS}/{id}";
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
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

        var uriSegment = $"{Constants.PROJECTS}";

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        client.Logger = logger;
        // Act
        var result = await client.PostAsync<CreateProjectKeyResponse>(uriSegment, null);

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

        var uriSegment = $"{Constants.PROJECTS}?{stringedSchema}";

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        client.Logger = logger;
        var content = AbstractRestClient.CreatePayload(source);

        // Act
        var result = await client.PostAsync<SyncPrerecordedResponse>(uriSegment, content);

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
        var content = AbstractRestClient.CreatePayload(source);
        var schema = new PrerecordedSchema { Punctuate = true, Utterances = true };
        var stringedSchema = QueryParameterUtil.GetParameters(schema);

        var uriSegment = $"{Constants.PROJECTS}?{stringedSchema}";

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        client.Logger = logger;
        // Act
        // Act & Assert       
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync<SyncPrerecordedResponse>(uriSegment, content));
        logger.Received().AnyLogOfType(LogLevel.Error, "Error occurred during POST request");
        return Task.CompletedTask;
    }


    [Test]
    public void DeleteAsync_Should_Return_Nothing_On_SuccessfulResponse()
    {
        // Arrange        
        var expectedResponse = new VoidResponse();
        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
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
        var expectedResponse = new VoidResponse()
        {
            Error = new Exception()
        };

        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
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

        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
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
        var uriSegment = $"{Constants.PROJECTS}";
        var client = new ConcreteRestClient("apiKey", new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        client.Logger = logger;
        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.DeleteAsync(uriSegment));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }


    [Test]
    public async Task PatchAsync_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange
        var id = new Faker().Random.Guid().ToString();
        var updateOptions = new AutoFaker<UpdateProjectSchema>().Generate();
        var uriSegment = $"{Constants.PROJECTS}/{id}";

        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        client.Logger = logger;
        var content = AbstractRestClient.CreatePayload(updateOptions);


        // Act
        var result = await client.PatchAsync<MessageResponse>(uriSegment, content);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.That(expectedResponse.Message, Is.EqualTo(result.Message));
    }

    [Test]
    public void PatchAsync_TResponse_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange
        var id = new Faker().Random.Guid().ToString();
        var updateOptions = new AutoFaker<UpdateProjectSchema>().Generate();
        var uriSegment = $"{Constants.PROJECTS}/{id}";
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        client.Logger = logger;

        var content = AbstractRestClient.CreatePayload(updateOptions);

        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.PatchAsync<MessageResponse>(uriSegment, content));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }

    [Test]
    public async Task PutAsync_Should_Return_ExpectedResult_On_SuccessfulResponse()
    {
        // Arrange
        var id = new Faker().Random.Guid().ToString();
        var updateOptions = new AutoFaker<UpdateProjectSchema>().Generate();
        var uriSegment = $"{Constants.PROJECTS}/{id}";

        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.OK);
        client.Logger = logger;

        var content = AbstractRestClient.CreatePayload(updateOptions);

        // Act
        var result = await client.PutAsync<MessageResponse>(uriSegment, content);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.That(expectedResponse.Message, Is.EqualTo(result.Message));
    }

    [Test]
    public void PutAsync_TResponse_Should_ThrowsException_On_UnsuccessfulResponse()
    {
        // Arrange
        var id = new Faker().Random.Guid().ToString();
        var updateOptions = new AutoFaker<UpdateProjectSchema>().Generate();
        var uriSegment = $"{Constants.PROJECTS}/{id}";
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        var client = new ConcreteRestClient(ApiKey, new DeepgramClientOptions(), httpClientFactory);
        client.HttpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse, HttpStatusCode.BadRequest);
        client.Logger = logger;
        var content = AbstractRestClient.CreatePayload(updateOptions);

        //Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.PutAsync<MessageResponse>(uriSegment, content));

        // Act & Assert

        logger.ReceivedWithAnyArgs();
    }
}
