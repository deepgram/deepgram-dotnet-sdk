// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Manage;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;
public class ManageClientTest
{
    DeepgramClientOptions _options;
    string _projectId;
    string _apiKey;
    [SetUp]
    public void Setup()
    {
        _options = new DeepgramClientOptions();
        _projectId = new Faker().Random.Guid().ToString();
        _apiKey = new Faker().Random.Guid().ToString();
    }

    #region Projects




    [Test]
    public async Task GetProjects_Should_Call_GetAsync_Returning_ProjectsResponse()
    {
        // Arrange
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);

        manageClient.When(x => x.GetAsync<ProjectsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<ProjectsResponse>(UriSegments.PROJECTS).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjects();

        // Assert
        await manageClient.Received().GetAsync<ProjectsResponse>(UriSegments.PROJECTS);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ProjectsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);

        }
    }

    [Test]
    public async Task GetProject_Should_Call_GetAsync_Returning_ProjectResponse()
    {

        // Arrange
        var expectedResponse = new AutoFaker<ProjectResponse>().Generate();
        expectedResponse.ProjectId = _projectId;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<ProjectResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<ProjectResponse>($"{UriSegments.PROJECTS}/{_projectId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProject(_projectId);

        // Assert
        await manageClient.Received().GetAsync<ProjectResponse>($"projects/{_projectId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ProjectResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task UpdateProject_Should_Call_PatchAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var updateProjectSchema = new AutoFaker<ProjectSchema>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PatchAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PatchAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}", Arg.Any<StringContent>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.UpdateProject(_projectId, updateProjectSchema);

        //Assert
        await manageClient.Received().PatchAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}", Arg.Any<StringContent>());
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

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/leave").Returns(expectedResponse);

        //Act
        var result = await manageClient.LeaveProject(_projectId);

        //Assert
        await manageClient.Received().DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/leave");

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

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProject(_projectId);

        // Assert
        await manageClient.Received().DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}");
    }

    #endregion

    #region ProjectKeys
    [Test]
    public async Task GetProjectKeys_Should_Call_GetAsync_Returning_KeysResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<KeysResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<KeysResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<KeysResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys").Returns(expectedResponse);

        //Act
        var result = await manageClient.GetProjectKeys(_projectId);

        //Assert
        await manageClient.Received().GetAsync<KeysResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeysResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectKey_Should_Call_GetAsync_Returning_KeyResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<KeyScopeResponse>().Generate();
        var keyId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<KeyScopeResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<KeyScopeResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}").Returns(expectedResponse);


        //Act
        var result = await manageClient.GetProjectKey(_projectId, keyId);
        await manageClient.Received().GetAsync<KeyScopeResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}");

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyScopeResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Call_PostAsync_Returning_KeyResponse_Without_Expiration_TimeToLive_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.ExpirationDate = null;
        createKeySchema.TimeToLiveInSeconds = null;

        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PostAsync<KeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);



        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Call_PostAsync_Returning_KeyResponse_With_Expiration_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.TimeToLiveInSeconds = null;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PostAsync<KeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);



        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Return_KeyResponse_Without_TimeToLive_Set()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.ExpirationDate = null;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PostAsync<KeyResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.CreateProjectKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeyResponse>($"{UriSegments.PROJECTS}/{_projectId}/keys", Arg.Any<StringContent>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateProjectKey_Should_Throw_ArgumentException_When_Both_Expiration_And_TimeToLive_Set()
    {
        //Arrange 
        var createKeySchema = new AutoFaker<KeySchema>().Generate();

        var manageClient = new ManageClient(_apiKey, _options) { _httpClientWrapper = new HttpClientWrapper(new HttpClient()) };

        //Act & Assert
        await manageClient.Invoking(y => y.CreateProjectKey(UriSegments.PROJECTS, createKeySchema))
             .Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task DeleteProjectKey_Should_Call_DeleteAsync()
    {
        //Arrange
        var keyId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProjectKey(_projectId, keyId);

        // Assert
        await manageClient.Received().DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}");
    }

    #endregion

    #region ProjectInvites

    [Test]
    public async Task GetProjectInvites_Should_Call_GetAsync_Returning_InvitesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<InvitesResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<InvitesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<InvitesResponse>($"{UriSegments.PROJECTS}/{_projectId}/invites").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectInvites(_projectId);

        // Assert
        await manageClient.Received().GetAsync<InvitesResponse>($"{UriSegments.PROJECTS}/{_projectId}/invites");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<InvitesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task SendProjectInvite_Should_Call_PostAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var inviteSchema = new AutoFaker<InviteSchema>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PostAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PostAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/invites", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.SendProjectInvite(_projectId, inviteSchema);

        // Assert
        await manageClient.Received().PostAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/invites", Arg.Any<StringContent>());
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
        var email = new Faker().Internet.Email();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/invites/{email}").Returns(Task.CompletedTask);

        // Act
        await manageClient.DeleteProjectInvite(_projectId, email);

        // Assert
        await manageClient.Received().DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/invites/{email}");

    }
    #endregion

    #region Members
    [Test]
    public async Task GetProjectMembers_Should_Call_GetAsync_Returning_MembersResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MembersResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<MembersResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<MembersResponse>($"{UriSegments.PROJECTS}/{_projectId}/members").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectMembers(_projectId);

        // Assert
        await manageClient.Received().GetAsync<MembersResponse>($"{UriSegments.PROJECTS}/{_projectId}/members");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MembersResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectMemberScopes_Should_Call_GetAsync_Returning_MemberScopesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MemberScopesResponse>().Generate();
        var memberId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);


        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<MemberScopesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<MemberScopesResponse>($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectMemberScopes(_projectId, memberId);

        // Assert
        await manageClient.Received().GetAsync<MemberScopesResponse>($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MemberScopesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task UpdateProjectMemberScope_Should_Call_PutAsync_Returning_MessageResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var memberScopeSchema = new AutoFaker<MemberScopeSchema>().Generate();
        var memberId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.PutAsync<MessageResponse>(Arg.Any<string>(), Arg.Any<StringContent>())).DoNotCallBase();
        manageClient.PutAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes", Arg.Any<StringContent>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.UpdateProjectMemberScope(_projectId, memberId, memberScopeSchema);

        // Assert
        await manageClient.Received().PutAsync<MessageResponse>($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes", Arg.Any<StringContent>());
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
        var memberId = new Faker().Random.Guid().ToString();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new VoidResponse());

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.DeleteAsync(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}").Returns(Task.CompletedTask);

        // Act
        await manageClient.RemoveProjectMember(_projectId, memberId);

        // Assert
        await manageClient.Received().DeleteAsync($"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}");

    }
    #endregion

    #region Usage
    [Test]
    public async Task GetProjectUsageRequests_Should_Call_GetAsync_Returning_UsageRequestsResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<UsageRequestsResponse>().Generate();
        var UsageRequestsSchema = new AutoFaker<UsageRequestsSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(UsageRequestsSchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<UsageRequestsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<UsageRequestsResponse>($"{UriSegments.PROJECTS}/{_projectId}/requests?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageRequests(_projectId, UsageRequestsSchema);

        // Assert
        await manageClient.Received().GetAsync<UsageRequestsResponse>($"{UriSegments.PROJECTS}/{_projectId}/requests?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageRequestsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectsUsageRequest_Should_Call_GetAsync_Returning_UsageRequestResponse()
    {
        //Arrange 
        var requestId = new Faker().Random.Guid().ToString();
        var expectedResponse = new AutoFaker<UsageRequestResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<UsageRequestResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<UsageRequestResponse>($"{UriSegments.PROJECTS}/{_projectId}/requests/{requestId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageRequest(_projectId, requestId);

        // Assert
        await manageClient.Received().GetAsync<UsageRequestResponse>($"{UriSegments.PROJECTS}/{_projectId}/requests/{requestId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageRequestResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectsUsageSummary_Should_Call_GetAsync_Returning_UsageSummaryResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<UsageSummaryResponse>().Generate();
        var getProjectUsageSummarySchema = new AutoFaker<UsageSummarySchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<UsageSummaryResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<UsageSummaryResponse>($"{UriSegments.PROJECTS}/{_projectId}/usage?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageSummary(_projectId, getProjectUsageSummarySchema);

        // Assert
        await manageClient.Received().GetAsync<UsageSummaryResponse>($"{UriSegments.PROJECTS}/{_projectId}/usage?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageSummaryResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectUsageFields_Should_GetAsync_Returning_UsageSummaryResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<UsageFieldsResponse>().Generate();
        var getProjectUsageFieldsSchema = new AutoFaker<UsageFieldsSchema>().Generate();
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<UsageFieldsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<UsageFieldsResponse>($"{UriSegments.PROJECTS}/{_projectId}/usage/fields?{stringedOptions}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectUsageFields(_projectId, getProjectUsageFieldsSchema);

        // Assert
        await manageClient.Received().GetAsync<UsageFieldsResponse>($"{UriSegments.PROJECTS}/{_projectId}/usage/fields?{stringedOptions}");

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageFieldsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    #endregion

    #region Balances

    [Test]
    public async Task GetProjectBalances_Should_Call_GetAsync_Returning_BalancesResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<BalancesResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);

        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        manageClient.When(x => x.GetAsync<BalancesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<BalancesResponse>($"{UriSegments.PROJECTS}/{_projectId}/balances").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectBalances(_projectId);

        // Assert
        await manageClient.Received().GetAsync<BalancesResponse>($"{UriSegments.PROJECTS}/{_projectId}/balances");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BalancesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetProjectBalance_Should_Call_GetAsync_Returning_BalanceResponse()
    {
        //Arrange 
        var expectedResponse = new AutoFaker<BalanceResponse>().Generate();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options);
        manageClient._httpClientWrapper = new HttpClientWrapper(httpClient);
        var balanceId = new Faker().Random.Guid().ToString();
        manageClient.When(x => x.GetAsync<BalanceResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<BalanceResponse>($"{UriSegments.PROJECTS}/{_projectId}/balances/{balanceId}").Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjectBalance(_projectId, balanceId);

        // Assert
        await manageClient.Received().GetAsync<BalanceResponse>($"{UriSegments.PROJECTS}/{_projectId}/balances/{balanceId}");
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BalanceResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }


    #endregion


}
