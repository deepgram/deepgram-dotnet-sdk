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
        var SUT = new ApiRequest(client, credentials);

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
        var SUT = new ApiRequest(client, credentials);
        var parameters = new AutoFaker<GetUsageSummaryOptions>().Generate();
        //Act
        var result = await SUT.SendHttpRequestAsync<UsageSummary>(HttpMethod.Get, "fake.com", null, parameters);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<UsageSummary>(result);
    }

    [Fact]
    public async void Should_Return_A_MessageResponse_When_HttpMethod_Put()
    {
        //Arrange
        var responseObject = new AutoFaker<MessageResponse>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials);
        var options = new AutoFaker<UpdateScopeOptions>().Generate();
        //Act
        var result = await SUT.SendHttpRequestAsync<MessageResponse>(HttpMethod.Put, "fake.com", options, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
    }

    [Fact]
    public async void Should_Return_A_PrerecorededTranscription_When_HttpMethod_Post()
    {
        //Arrange
        var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials);
        var source = new AutoFaker<UrlSource>().Generate();
        var options = new PrerecordedTranscriptionOptionsFaker().Generate();
        //Act
        var result = await SUT.SendHttpRequestAsync<PrerecordedTranscription>(HttpMethod.Post, "fake.com", source, options);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<PrerecordedTranscription>(result);
    }


    [Fact]
    public async void Should_Return_A_MessageResponse_When_HttpMethod_Patch()
    {
        //Arrange
        var responseObject = new AutoFaker<MessageResponse>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials);
        var body = new AutoFaker<Project>().Generate();
        //Act
        var result = await SUT.SendHttpRequestAsync<MessageResponse>(HttpMethod.Patch, "fake.com", body, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
    }

    [Fact]
    public async void Should_Return_A_MessageResponse_When_HttpMethod_Delete()
    {
        //Arrange
        var responseObject = new AutoFaker<MessageResponse>().Generate();
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var SUT = new ApiRequest(client, credentials);

        //Act
        var result = await SUT.SendHttpRequestAsync<MessageResponse>(HttpMethod.Delete, "fake.com", null, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
    }



}