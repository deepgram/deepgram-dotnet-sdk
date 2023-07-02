namespace Deepgram.Tests.ClientTests;

public class KeyClientTests
{


    [Fact]
    public async void ListKeysAsync_Should_Return_KeyList()
    {
        //Arrange
        var returnObject = new AutoFaker<KeyList>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Keys.ListKeysAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<KeyList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void GetKeyAsync_Should_Return_Key()
    {
        //Arrange 
        var returnObject = new AutoFaker<Key>().Generate();
        var SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var keyId = faker.Random.Guid().ToString();

        //Act
        var result = await SUT.Keys.GetKeyAsync(projectId, keyId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<Key>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_Without_CreateKeyOptions()
    {
        //Arrange
        var returnObject = new AutoFaker<ApiKey>().Generate();
        DeepgramClient SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_With_CreateKeyOptions_With_ExpirationDate_Set()
    {
        //Arrange
        var returnObject = new AutoFaker<ApiKey>().Generate();
        var options = new CreateKeyOptions() { ExpirationDate = DateTime.Now };
        DeepgramClient SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, options);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void CreateKeyAsync_Should_Return_ApiKey_With_CreateKeyOptions_With_TimeToLive_Set()
    {
        //Arrange
        var returnObject = new AutoFaker<ApiKey>().Generate();
        var options = new CreateKeyOptions() { TimeToLive = 30 };
        DeepgramClient SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var scopes = faker.Random.WordsArray(1, 3);
        var comment = faker.Lorem.Sentence();

        //Act
        var result = await SUT.Keys.CreateKeyAsync(projectId, comment, scopes, options);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ApiKey>(result);
        Assert.Equal(returnObject, result);
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


    [Fact]
    public async void DeleteKeyAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);

        var faker = new Faker();
        var projectId = faker.Random.Guid().ToString();
        var keyId = faker.Random.Guid().ToString();

        //Act
        var result = await SUT.Keys.DeleteKeyAsync(projectId, keyId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }



    private static DeepgramClient GetDeepgramClient<T>(T returnObject)
    {
        var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
        var mockIApiRequest = MockIApiRequest.Create(returnObject);
        var credentials = new CredentialsFaker().Generate();
        var SUT = new DeepgramClient(credentials);
        SUT.Keys.ApiRequest = mockIApiRequest.Object;
        SUT.Keys.RequestMessageBuilder = mockIRequestMessageBuilder.Object;
        return SUT;
    }


}
