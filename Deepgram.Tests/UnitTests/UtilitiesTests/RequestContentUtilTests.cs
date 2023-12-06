
namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class RequestContentUtilTests
{
    [Test]
    public void CreatePayload_Should_Return_StringContent()
    {

        //Arrange       
        var project = new AutoFaker<Project>().Generate();

        //Act
        var result = RequestContentUtil.CreatePayload(project);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<StringContent>();
        }
    }


    [Test]
    public void CreateStreamPayload_Should_Return_HttpContent()
    {
        //Arrange 
        var source = System.Text.Encoding.ASCII.GetBytes("Acme Unit Testing");
        var stream = new MemoryStream(source);

        //Act
        var result = RequestContentUtil.CreateStreamPayload(stream);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<HttpContent>();
        }
    }


    [Test]
    public async Task Create()
    {
        //Arrange 
        var obj = new GetProjectResponse()
        {
            ProjectId = "1",
            Name = "Test"
        };
        var serialized = JsonSerializer.Serialize(obj);
        var deser = JsonSerializer.Deserialize<GetProjectResponse>(serialized);
        var stream = new MemoryStream();
        var apiKey = "2134213";
        var httpClient = MockHttpClient.CreateHttpClientWithResult(obj, HttpStatusCode.OK);
        var hcf = Substitute.For<IHttpClientFactory>();
        hcf.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);
        var client = new ConcreteRestClient(apiKey, hcf);

        // Act
        var result = await client.GetAsync<GetProjectsResponse>(Constants.PROJECTS);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectsResponse>();
            // result.Should().BeEquivalentTo(expectedResponse);
        };
        //Act
        //var result = RequestContentUtil.CreateStreamPayload(stream);

        //Assert
        using (new AssertionScope())
        {
            serialized.Should().NotBeNull();
            // result.Should().BeAssignableTo<HttpContent>();
        }
    }
}
