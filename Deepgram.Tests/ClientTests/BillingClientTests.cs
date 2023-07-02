namespace Deepgram.Tests.ClientTests;

public class BillingClientTests
{
    [Fact]
    public async void GetAllBalancesAsync_Should_Return_BillingList()
    {
        //Arrange
        var returnObject = new AutoFaker<BillingList>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString(); ;

        //Act
        var result = await SUT.Billing.GetAllBalancesAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<BillingList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void GetBalanceAsync_Should_Return_Billing()
    {
        //Arrange
        var returnObject = new AutoFaker<Billing>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();
        var balanceId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Billing.GetBalanceAsync(projectId, balanceId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<Billing>(result);
        Assert.Equal(returnObject, result);
    }


    private static DeepgramClient GetDeepgramClient<T>(T returnObject)
    {
        var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
        var mockIApiRequest = MockIApiRequest.Create(returnObject);
        var credentials = new CredentialsFaker().Generate();
        var SUT = new DeepgramClient(credentials);
        SUT.Billing.ApiRequest = mockIApiRequest.Object;
        SUT.Billing.RequestMessageBuilder = mockIRequestMessageBuilder.Object;
        return SUT;
    }
}
