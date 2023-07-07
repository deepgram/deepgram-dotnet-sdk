namespace Deepgram.Tests.RequestTests;

public class ApiRequestTests
{
    [Fact]

    public async void Should_Return_A_Project_When_HttpMethod_Get_Without_Parameters()
    {
        //Arrange
        var responseObject = new AutoFaker<Project>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials, new RequestMessageBuilder());

        //Act
        var result = await SUT.SendHttpRequestAsync<Project>(HttpMethod.Get, "fake.com", null, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<Project>(result);
    }

    [Fact]

    public async void Should_Return_A_object_When_HttpMethod_Get_With_Parameters()
    {
        //Arrange
        var responseObject = new AutoFaker<UsageSummary>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials, new RequestMessageBuilder());
        var parameters = new AutoFaker<GetUsageSummaryOptions>().Generate();
        //Act
        var result = await SUT.SendHttpRequestAsync<UsageSummary>(HttpMethod.Get, "fake.com", null, parameters);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<UsageSummary>(result);
    }







}