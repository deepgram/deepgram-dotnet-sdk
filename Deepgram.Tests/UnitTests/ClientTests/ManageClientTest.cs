using Deepgram.Records;

namespace Deepgram.Tests.UnitTests.ClientTests;
public class ManageClientTest
{
    DeepgramClientOptions _options;
    string _projectId;
    readonly string _urlPrefix = $"/{Constants.API_VERSION}/{Constants.PROJECTS}";
    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions("fake");
        _projectId = new Faker().Random.Guid().ToString();
    }

    #region Projects

    [Test]
    public void ManageClient_urlPrefix_Should_Match_Expected()
    {
        //Arrange 
        var expected = "/v1/projects";
        var client = new ManageClient(_options, new HttpClient());


        //Assert
        using (new AssertionScope())
        {
            client.UrlPrefix.Should().NotBeNull();
            client.UrlPrefix.Should().BeEquivalentTo(expected);
        }
    }



    [Test]
    public async Task GetProjects_Should_Call_GetAsync_Returning_GetProjectsResponse()
    {
        // Arrange
        var expectedResponse = new AutoFaker<GetProjectsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectsResponse>(_urlPrefix).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjects();

        // Assert
        await manageClient.Received().GetAsync<GetProjectsResponse>(_urlPrefix);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);

        }
    }

    [Test]
    public async Task GetProject_Should_Call_GetAsync_Returning_GetProjectResponse()
    {

        // Arrange
        var expectedResponse = new AutoFaker<GetProjectResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectResponse>($"{_urlPrefix}/{_projectId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProject(_projectId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectResponse>($"/v1/projects/{_projectId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task UpdateProject_Should_Call_PatchAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var updateProjectSchema = new AutoFaker<UpdateProjectSchema>().Generate();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);

        manageClient.When(x => x.PatchAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PatchAsync<MessageResponse>($"{_urlPrefix}/{_projectId}", Arg.Any<StringContent>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.UpdateProject(_projectId, updateProjectSchema);

        //Assert
        await manageClient.Received().PatchAsync<MessageResponse>($"{_urlPrefix}/{_projectId}", Arg.Any<StringContent>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task LeaveProject_Should_Call_DeleteAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/leave").Returns(expectedResponse);

        //Act
        var result = await manageClient.LeaveProject(_projectId);

        //Assert
        await manageClient.Received().DeleteAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/leave");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteProject_Should_Call_DeleteAsync()
    {
        //Arrange 
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{_urlPrefix}/{_projectId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProject(_projectId);

        // Assert
        await manageClient.Received().DeleteAsync($"{_urlPrefix}/{_projectId}");

    }

    #endregion

    #region ProjectKeys
    [Test]
    public async Task GetProjectKeys_Should_Call_GetAsync_Returning_GetProjectKeysResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectKeysResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectKeysResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectKeysResponse>($"{_urlPrefix}/{_projectId}/keys").Returns(expectedResponse);


        //Act
        var result = await manageClient.GetProjectKeys(_projectId);

        //Assert
        await manageClient.Received().GetAsync<GetProjectKeysResponse>($"{_urlPrefix}/{_projectId}/keys");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectKeysResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectKey_Should_Call_GetAsync_Returning_GetProjectKeyResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectKeyResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var keyId = new Faker().Random.Guid().ToString();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectKeyResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys/{keyId}").Returns(expectedResponse);



        //Act
        var result = await manageClient.GetProjectKey(_projectId, keyId);
        await manageClient.Received().GetAsync<GetProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys/{keyId}");

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectKeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Call_PostAsync_Returning_CreateProjectKeyResponse_Without_Expiration_TimeToLive_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<CreateProjectKeyResponse>().Generate();
        var createKeySchema = new AutoFaker<CreateProjectKeySchema>().Generate();
        createKeySchema.ExpirationDate = null;
        createKeySchema.TimeToLiveInSeconds = null;

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.PostAsync<CreateProjectKeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);



        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CreateProjectKeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Call_PostAsync_Returning_CreateProjectKeyResponse_With_Expiration_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<CreateProjectKeyResponse>().Generate();
        var createKeySchema = new AutoFaker<CreateProjectKeySchema>().Generate();
        createKeySchema.TimeToLiveInSeconds = null;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.PostAsync<CreateProjectKeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);



        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CreateProjectKeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Return_CreateProjectKeyResponse_Without_TimeToLive_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<CreateProjectKeyResponse>().Generate();
        var createKeySchema = new AutoFaker<CreateProjectKeySchema>().Generate();
        createKeySchema.ExpirationDate = null;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.PostAsync<CreateProjectKeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<CreateProjectKeyResponse>($"{_urlPrefix}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CreateProjectKeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Throw_ArgumentException_When_Both_Expiration_And_TimeToLive_Set()
    {
        //Arrange 
        var createKeySchema = new AutoFaker<CreateProjectKeySchema>().Generate();
        var client = new ManageClient(_options, new HttpClient());

        //Act & Assert
        await client.Invoking(y => y.CreateProjectKey(Constants.PROJECTS, createKeySchema))
             .Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task DeleteProjectKey_Should_Call_DeleteAsync()
    {
        //Arrange 
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var keyId = new Faker().Random.Guid().ToString();
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{_urlPrefix}/{_projectId}/keys/{keyId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProjectKey(_projectId, keyId);

        // Assert
        await manageClient.Received().DeleteAsync($"{_urlPrefix}/{_projectId}/keys/{keyId}");

    }

    #endregion

    #region ProjectInvites

    [Test]
    public async Task GetProjectInvites_Should_Call_GetAsync_Returning_GetProjectInvitesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectInvitesResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectInvitesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectInvitesResponse>($"{_urlPrefix}/{_projectId}/invites").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectInvites(_projectId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectInvitesResponse>($"{_urlPrefix}/{_projectId}/invites");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectInvitesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task SendProjectInvite_Should_Call_PostAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var sendProjectInviteSchema = new AutoFaker<SendProjectInviteSchema>().Generate();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.PostAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/invites", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.SendProjectInvite(_projectId, sendProjectInviteSchema);

        // Assert
        await manageClient.Received().PostAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/invites", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteProjectInvite_Should_Call_DeleteAsync()
    {
        //Arrange        
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var email = new Faker().Internet.Email();
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{_urlPrefix}/{_projectId}/invites/{email}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProjectInvite(_projectId, email);

        // Assert
        await manageClient.Received().DeleteAsync($"{_urlPrefix}/{_projectId}/invites/{email}");

    }
    #endregion

    #region Members
    [Test]
    public async Task GetProjectMembers_Should_Call_GetAsync_Returning_GetProjectMembersResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectMembersResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectMembersResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectMembersResponse>($"{_urlPrefix}/{_projectId}/members").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectMembers(_projectId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectMembersResponse>($"{_urlPrefix}/{_projectId}/members");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectMembersResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectMemberScopes_Should_Call_GetAsync_Returning_GetProjectMemberScopesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectMemberScopesResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var memberId = new Faker().Random.Guid().ToString();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.GetAsync<GetProjectMemberScopesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectMemberScopesResponse>($"{_urlPrefix}/{_projectId}/members/{memberId}/scopes").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectMemberScopes(_projectId, memberId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectMemberScopesResponse>($"{_urlPrefix}/{_projectId}/members/{memberId}/scopes");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectMemberScopesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task UpdateProjectMemberScope_Should_Call_PutAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var updateProjectMemberScopeSchema = new AutoFaker<UpdateProjectMemberScopeSchema>().Generate();
        var memberId = new Faker().Random.Guid().ToString();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        manageClient.When(x => x.PutAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PutAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/members/{memberId}/scopes", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.UpdateProjectMemberScope(_projectId, memberId, updateProjectMemberScopeSchema);

        // Assert
        await manageClient.Received().PutAsync<MessageResponse>($"{_urlPrefix}/{_projectId}/members/{memberId}/scopes", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task RemoveProjectMember_Should_Call_DeleteAsync()
    {
        //Arrange         
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var memberId = new Faker().Random.Guid().ToString();
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{_urlPrefix}/{_projectId}/members/{memberId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.RemoveProjectMember(_projectId, memberId);

        // Assert
        await manageClient.Received().DeleteAsync($"{_urlPrefix}/{_projectId}/members/{memberId}");

    }
    #endregion

    #region Usage
    [Test]
    public async Task GetProjectUsageRequests_Should_Call_GetAsync_Returning_GetProjectUsageRequestsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectUsageRequestsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var getProjectUsageRequestsSchema = new AutoFaker<GetProjectUsageRequestsSchema>().Generate();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);

        manageClient.When(x => x.GetAsync<GetProjectUsageRequestsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectUsageRequestsResponse>($"{_urlPrefix}/{_projectId}/requests?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageRequests(_projectId, getProjectUsageRequestsSchema);

        // Assert
        await manageClient.Received().GetAsync<GetProjectUsageRequestsResponse>($"{_urlPrefix}/{_projectId}/requests?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectUsageRequestsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectsUsageRequest_Should_Call_GetAsync_Returning_GetProjectUsageRequestResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectUsageRequestResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var requestId = new Faker().Random.Guid().ToString();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);

        manageClient.When(x => x.GetAsync<GetProjectUsageRequestResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectUsageRequestResponse>($"{_urlPrefix}/{_projectId}/requests/{requestId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageRequest(_projectId, requestId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectUsageRequestResponse>($"{_urlPrefix}/{_projectId}/requests/{requestId}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectUsageRequestResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectsUsageSummary_Should_Call_GetAsync_Returning_GetProjectUsageSummaryResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectUsageSummaryResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var getProjectUsageSummarySchema = new AutoFaker<GetProjectsUsageSummarySchema>().Generate();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);

        manageClient.When(x => x.GetAsync<GetProjectUsageSummaryResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectUsageSummaryResponse>($"{_urlPrefix}/{_projectId}/usage?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageSummary(_projectId, getProjectUsageSummarySchema);

        // Assert
        await manageClient.Received().GetAsync<GetProjectUsageSummaryResponse>($"{_urlPrefix}/{_projectId}/usage?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectUsageSummaryResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectUsageFields_Should_GetAsync_Returning_GetProjectUsageSummaryResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectUsageFieldsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var getProjectUsageFieldsSchema = new AutoFaker<GetProjectUsageFieldsSchema>().Generate();
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);

        manageClient.When(x => x.GetAsync<GetProjectUsageFieldsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectUsageFieldsResponse>($"{_urlPrefix}/{_projectId}/usage/fields?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageFields(_projectId, getProjectUsageFieldsSchema);

        // Assert
        await manageClient.Received().GetAsync<GetProjectUsageFieldsResponse>($"{_urlPrefix}/{_projectId}/usage/fields?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectUsageFieldsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    #endregion

    #region Balances

    [Test]
    public async Task GetProjectBalances_Should_Call_GetAsync_Returning_GetProjectBalancesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectBalancesResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);

        manageClient.When(x => x.GetAsync<GetProjectBalancesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectBalancesResponse>($"{_urlPrefix}/{_projectId}/balances").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectBalances(_projectId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectBalancesResponse>($"{_urlPrefix}/{_projectId}/balances");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectBalancesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectBalance_Should_Call_GetAsync_Returning_GetProjectBalanceResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<GetProjectBalanceResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_options, httpClient);
        var balanceId = new Faker().Random.Guid().ToString();
        manageClient.When(x => x.GetAsync<GetProjectBalanceResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<GetProjectBalanceResponse>($"{_urlPrefix}/{_projectId}/balances/{balanceId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectBalance(_projectId, balanceId);

        // Assert
        await manageClient.Received().GetAsync<GetProjectBalanceResponse>($"{_urlPrefix}/{_projectId}/balances/{balanceId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<GetProjectBalanceResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }


    #endregion


}
