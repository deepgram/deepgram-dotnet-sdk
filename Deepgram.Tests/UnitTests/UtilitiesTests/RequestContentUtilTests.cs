using Deepgram.Models;

namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class RequestContentUtilTests
{
    [Test]
    public void CreatePayload_Should_Return_StringContent()
    {

        //Arrange       
        var project = new AutoFaker<Project>().Generate();

        //Act
        var result = RequestContentUtil.CreatePayload("TestClient", project);

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
}
