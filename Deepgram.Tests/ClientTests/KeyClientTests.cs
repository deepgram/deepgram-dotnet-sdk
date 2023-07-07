namespace Deepgram.Tests.ClientTests;
public class KeyClientTests
{

    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_Without_CreateKeyOptions()
    {
        //Arrange

        var responseObject = new AutoFaker<ApiKey>().Generate();
        DeepgramClient SUT = GetDeepgramClient(responseObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(responseObject, result);
    }


    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_With_CreateKeyOptions_With_ExpirationDate_Set()
    {
        //Arrange
        var responseObject = new AutoFaker<ApiKey>().Generate();
        var options = new CreateKeyOptions() { ExpirationDate = DateTime.Now };
        DeepgramClient SUT = GetDeepgramClient(responseObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, options);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(responseObject, result);
    }

    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_With_CreateKeyOptions_With_TimeToLive_Set()
    {
        //Arrange       
        var options = new CreateKeyOptions() { TimeToLive = 30 };
        var responseObject = new AutoFaker<ApiKey>().Generate();
        DeepgramClient SUT = GetDeepgramClient(responseObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, options);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(responseObject, result);
    }

    [Fact]
    public async void CreateKeyAsync_Should_Throw_Exception_When_Both_ExpirationDate_And_TimeToLive_Set()
    {
        //Arrange
        var returnObject = new AutoFaker<ApiKey>().Generate();
        var options = new CreateKeyOptions() { ExpirationDate = DateTime.Now, TimeToLive = 30 };
        DeepgramClient SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await Assert.ThrowsAsync<ArgumentException>(() => _ = SUT.Keys.CreateKeyAsync(projectId, comment, scopes, options));

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ArgumentException>(result);
        Assert.Equal(" Please provide expirationDate or timeToLive or neither. Providing both is not allowed.", result.Message);

    }
    private static DeepgramClient GetDeepgramClient(ApiKey responseObject)
    {
        var credentials = new CleanCredentialsFaker().Generate();
        var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
        var MockApi = MockIApiRequest.Create(responseObject);
        DeepgramClient SUT = new DeepgramClient(new Credentials() { ApiKey = "sdadad" });
        SUT._apiRequest = MockApi.Object;
        SUT.InitializeClients();
        return SUT;
    }


}
