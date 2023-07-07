namespace Deepgram.Tests.ClientTests;

public class ProjectClientTests
{
    [Fact]
    public async void ListProjectsAsync_Should_Return_ProjectList()
    {
        //Arrange
        var returnObject = new AutoFaker<ProjectList>().Generate();
        var SUT = GetDeepgramClient(returnObject);

        //Act
        var result = await SUT.Projects.ListProjectsAsync();

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ProjectList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void GetProjectAsync_Should_Return_Project()
    {
        //Arrange
        var returnObject = new AutoFaker<Project>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Projects.GetProjectAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<Project>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void UpdateProjectAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var project = new AutoFaker<Project>().Generate();
        //Act
        var result = await SUT.Projects.UpdateProjectAsync(project);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void DeleteProjectAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Projects.DeleteProjectAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void GetMembersAsync_Should_Return_MembersList()
    {
        //Arrange
        var returnObject = new AutoFaker<MemberList>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString(); ;

        //Act
        var result = await SUT.Projects.GetMembersAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MemberList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void GetMemberScopeAsync_Should_Return_ScopesList()
    {
        //Arrange
        var returnObject = new AutoFaker<ScopesList>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();
        var memberId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Projects.GetMemberScopesAsync(projectId, memberId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ScopesList>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void LeaveProjectAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();


        //Act
        var result = await SUT.Projects.LeaveProjectAsync(projectId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void RemoveMemberAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();
        var memberId = new Faker().Random.Guid().ToString();

        //Act
        var result = await SUT.Projects.RemoveMemberAsync(projectId, memberId);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }

    [Fact]
    public async void UpdateScopeAsync_Should_Return_MessageResponse()
    {
        //Arrange
        var returnObject = new AutoFaker<MessageResponse>().Generate();
        var SUT = GetDeepgramClient(returnObject);
        var projectId = new Faker().Random.Guid().ToString();
        var memberId = new Faker().Random.Guid().ToString();
        var updateScopes = new AutoFaker<UpdateScopeOptions>().Generate();
        //Act
        var result = await SUT.Projects.UpdateScopeAsync(projectId, memberId, updateScopes);

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<MessageResponse>(result);
        Assert.Equal(returnObject, result);
    }

    private static DeepgramClient GetDeepgramClient<T>(T returnObject)
    {
        var mockIApiRequest = MockIApiRequest.Create(returnObject);
        var credentials = new CredentialsFaker().Generate();
        var SUT = new DeepgramClient(credentials);
        SUT.Projects.ApiRequest = mockIApiRequest.Object;
        return SUT;
    }
}
