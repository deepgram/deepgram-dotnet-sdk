namespace Deepgram.Tests.ClientTests;

public class InvitationClientTests
{
    [Fact]
    public async void ListInvitationsAsync_Should_Return_InvitationList()
    {
        //Arrange
        var returnObject = new AutoFaker<InvitationList>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Invitation.ListInvitationsAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<InvitationList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void SendInvitation_Should_Return_InvitationResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<InvitationResponse>().Generate();
        var invitationOptions = new AutoFaker<InvitationOptions>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Invitation.SendInvitationAsync(projectId, invitationOptions);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<InvitationResponse>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void DeleteInvitation_Should_Return_MessageResponse()
    {
        var deepgramclient = new DeepgramClient(new Credentials() { ApiKey = "9a5355612e23754e2ff55cb2b5226c7026e517b8" });
        var projects = deepgramclient.Projects.ListProjectsAsync().Result;
        var project = projects.Projects[0];
        var response = await deepgramclient.Invitation.ListInvitationsAsync(project.Id);
        var options = response.Invites[0];
        var delete = deepgramclient.Invitation.DeleteInvitationAsync(project.Id, options.Email);
        var response2 = await deepgramclient.Invitation.ListInvitationsAsync(project.Id);

        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var email = new Faker().Internet.Email();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Invitation.DeleteInvitationAsync(projectId, email);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<InvitationResponse>(result);
        Assert.Equal(returnObject, result);
    }


    private static DeepgramClient GetDeepgramClient<T>(T returnObject)
    {
        var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
        var mockIApiRequest = MockIApiRequest.Create(returnObject);
        var credentials = new CredentialsFaker().Generate();
        var SUT = new DeepgramClient(credentials);

        SUT.Invitation.ApiRequest = mockIApiRequest.Object;
        SUT.Invitation.RequestMessageBuilder = mockIRequestMessageBuilder.Object;
        return SUT;
    }
}