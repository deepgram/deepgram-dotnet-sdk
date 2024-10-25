// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Clients.Manage.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ManageClientTest
{
    DeepgramHttpClientOptions _options;
    string _projectId;
    string _apiKey;
    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey)
        {
            OnPrem = true,
        };
        _projectId = new Faker().Random.Guid().ToString();
    }

    #region Projects
    [Test]
    public async Task GetProjects_Should_Call_GetAsync_Returning_ProjectsResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}");
        var expectedResponse = new AutoFaker<ProjectsResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);

        // Mock methods
        manageClient.When(x => x.GetAsync<ProjectsResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<ProjectsResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProjects();

        // Assert
        await manageClient.Received().GetAsync<ProjectsResponse>(url);
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}");
        var expectedResponse = new AutoFaker<ProjectResponse>().Generate();
        expectedResponse.ProjectId = _projectId;

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<ProjectResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<ProjectResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetProject(_projectId);

        // Assert
        await manageClient.Received().GetAsync<ProjectResponse>(url);
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
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var updateProjectSchema = new AutoFaker<ProjectSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.PatchAsync<ProjectSchema, MessageResponse>(Arg.Any<string>(), Arg.Any<ProjectSchema>())).DoNotCallBase();
        manageClient.PatchAsync<ProjectSchema, MessageResponse>(url, Arg.Any<ProjectSchema>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.UpdateProject(_projectId, updateProjectSchema);

        //Assert
        await manageClient.Received().PatchAsync<ProjectSchema, MessageResponse>(url, Arg.Any<ProjectSchema>());
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
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/leave");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        //Act
        var result = await manageClient.LeaveProject(_projectId);

        //Assert
        await manageClient.Received().DeleteAsync<MessageResponse>(url);

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
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.DeleteAsync< MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        // Act
        await manageClient.DeleteProject(_projectId);

        // Assert
        await manageClient.Received().DeleteAsync<MessageResponse>(url);
    }

    #endregion

    #region ProjectKeys
    [Test]
    public async Task GetKeys_Should_Call_GetAsync_Returning_KeysResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys");
        var expectedResponse = new AutoFaker<KeysResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<KeysResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<KeysResponse>(url).Returns(expectedResponse);

        //Act
        var result = await manageClient.GetKeys(_projectId);

        //Assert
        await manageClient.Received().GetAsync<KeysResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeysResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetKey_Should_Call_GetAsync_Returning_KeyResponse()
    {
        // Input and Output
        var keyId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}");
        var expectedResponse = new AutoFaker<KeyScopeResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<KeyScopeResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<KeyScopeResponse>(url).Returns(expectedResponse);

        //Act
        var result = await manageClient.GetKey(_projectId, keyId);
        await manageClient.Received().GetAsync<KeyScopeResponse>(url);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyScopeResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateKey_Should_Call_PostAsync_Returning_KeyResponse_Without_Expiration_TimeToLive_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys");
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.ExpirationDate = null;
        createKeySchema.TimeToLiveInSeconds = null;

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.PostAsync<KeySchema, KeyResponse>(Arg.Any<string>(), Arg.Any<KeySchema>())).DoNotCallBase();
        manageClient.PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.CreateKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateKey_Should_Call_PostAsync_Returning_KeyResponse_With_Expiration_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys");
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.TimeToLiveInSeconds = null;

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);

        // Mock methods
        manageClient.When(x => x.PostAsync<KeySchema, KeyResponse>(Arg.Any<string>(), Arg.Any<KeySchema>())).DoNotCallBase();
        manageClient.PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.CreateKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateKey_Should_Return_KeyResponse_Without_TimeToLive_Set()
    {
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys");
        var expectedResponse = new AutoFaker<KeyResponse>().Generate();
        var createKeySchema = new AutoFaker<KeySchema>().Generate();
        createKeySchema.ExpirationDate = null;

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.PostAsync<KeySchema, KeyResponse>(Arg.Any<string>(), Arg.Any<KeySchema>())).DoNotCallBase();
        manageClient.PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>()).Returns(expectedResponse);

        //Act
        var result = await manageClient.CreateKey(_projectId, createKeySchema);

        //Assert
        await manageClient.Received().PostAsync<KeySchema, KeyResponse>(url, Arg.Any<KeySchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<KeyResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task CreateKey_Should_Throw_ArgumentException_When_Both_Expiration_And_TimeToLive_Set()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}");
        var createKeySchema = new AutoFaker<KeySchema>().Generate();

        // Fake Client
        var manageClient = new ManageClient(_apiKey, _options, null);

        //Act & Assert
        await manageClient.Invoking(y => y.CreateKey(url, createKeySchema))
             .Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task DeleteKey_Should_Call_DeleteAsync()
    {
        // Input and Output
        var keyId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/keys/{keyId}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        // Act
        await manageClient.DeleteKey(_projectId, keyId);

        // Assert
        await manageClient.Received().DeleteAsync<MessageResponse>(url);
    }

    #endregion

    #region ProjectInvites
    [Test]
    public async Task GetInvites_Should_Call_GetAsync_Returning_InvitesResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/invites");
        var expectedResponse = new AutoFaker<InvitesResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<InvitesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<InvitesResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetInvites(_projectId);

        // Assert
        await manageClient.Received().GetAsync<InvitesResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<InvitesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task SendInvite_Should_Call_PostAsync_Returning_MessageResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/invites");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var inviteSchema = new AutoFaker<InviteSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.PostAsync<InviteSchema, MessageResponse>(Arg.Any<string>(), Arg.Any<InviteSchema>())).DoNotCallBase();
        manageClient.PostAsync<InviteSchema, MessageResponse>(url, Arg.Any<InviteSchema>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.SendInvite(_projectId, inviteSchema);

        // Assert
        await manageClient.Received().PostAsync<InviteSchema, MessageResponse>(url, Arg.Any<InviteSchema>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteInvite_Should_Call_DeleteAsync()
    {
        // Input and Output
        var email = new Faker().Internet.Email();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/invites/{email}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        // Act
        await manageClient.DeleteInvite(_projectId, email);

        // Assert
        await manageClient.Received().DeleteAsync<MessageResponse>(url);

    }
    #endregion

    #region Members
    [Test]
    public async Task GetMembers_Should_Call_GetAsync_Returning_MembersResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/members");
        var expectedResponse = new AutoFaker<MembersResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<MembersResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<MembersResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetMembers(_projectId);

        // Assert
        await manageClient.Received().GetAsync<MembersResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MembersResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetMemberScopes_Should_Call_GetAsync_Returning_MemberScopesResponse()
    {
        // Input and Output
        var memberId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes");
        var expectedResponse = new AutoFaker<MemberScopesResponse>().Generate();
        
        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<MemberScopesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<MemberScopesResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetMemberScopes(_projectId, memberId);

        // Assert
        await manageClient.Received().GetAsync<MemberScopesResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MemberScopesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task UpdateMemberScope_Should_Call_PutAsync_Returning_MessageResponse()
    {
        // Input and Output
        var memberId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}/scopes");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        var memberScopeSchema = new AutoFaker<MemberScopeSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.PutAsync<MemberScopeSchema, MessageResponse>(Arg.Any<string>(), Arg.Any<MemberScopeSchema>())).DoNotCallBase();
        manageClient.PutAsync<MemberScopeSchema, MessageResponse>(url, Arg.Any<MemberScopeSchema>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.UpdateMemberScope(_projectId, memberId, memberScopeSchema);

        // Assert
        await manageClient.Received().PutAsync<MemberScopeSchema, MessageResponse>(url, Arg.Any<MemberScopeSchema>());
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task RemoveMember_Should_Call_DeleteAsync()
    {
        // Input and Output
        var memberId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/members/{memberId}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        // Act
        await manageClient.RemoveMember(_projectId, memberId);

        // Assert
        await manageClient.Received().DeleteAsync<MessageResponse>(url);

    }
    #endregion

    #region Usage
    [Test]
    public async Task GetUsageRequests_Should_Call_GetAsync_Returning_UsageRequestsResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/requests");
        var expectedResponse = new AutoFaker<UsageRequestsResponse>().Generate();
        var UsageRequestsSchema = new AutoFaker<UsageRequestsSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<UsageRequestsSchema, UsageRequestsResponse>(Arg.Any<string>(), Arg.Any<UsageRequestsSchema>())).DoNotCallBase();
        manageClient.GetAsync<UsageRequestsSchema, UsageRequestsResponse>(url, Arg.Any<UsageRequestsSchema>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetUsageRequests(_projectId, UsageRequestsSchema);

        // Assert
        await manageClient.Received().GetAsync<UsageRequestsSchema, UsageRequestsResponse>(url, Arg.Any<UsageRequestsSchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageRequestsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetsUsageRequest_Should_Call_GetAsync_Returning_UsageRequestResponse()
    {
        // Input and Output
        var requestId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/requests/{requestId}");
        var expectedResponse = new AutoFaker<UsageRequestResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<UsageRequestResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<UsageRequestResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetUsageRequest(_projectId, requestId);

        // Assert
        await manageClient.Received().GetAsync<UsageRequestResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageRequestResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetsUsageSummary_Should_Call_GetAsync_Returning_UsageSummaryResponse()
    {
        // Input and Output 
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/usage");
        var expectedResponse = new AutoFaker<UsageSummaryResponse>().Generate();
        var getProjectUsageSummarySchema = new AutoFaker<UsageSummarySchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<UsageSummarySchema, UsageSummaryResponse>(Arg.Any<string>(), Arg.Any<UsageSummarySchema>())).DoNotCallBase();
        manageClient.GetAsync<UsageSummarySchema, UsageSummaryResponse>(url, Arg.Any<UsageSummarySchema>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetUsageSummary(_projectId, getProjectUsageSummarySchema);

        // Assert
        await manageClient.Received().GetAsync<UsageSummarySchema, UsageSummaryResponse>(url, Arg.Any<UsageSummarySchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<UsageSummaryResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetUsageFields_Should_GetAsync_Returning_UsageSummaryResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/usage/fields");
        var expectedResponse = new AutoFaker<UsageFieldsResponse>().Generate();
        var getProjectUsageFieldsSchema = new AutoFaker<UsageFieldsSchema>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<UsageFieldsSchema, UsageFieldsResponse>(Arg.Any<string>(), Arg.Any<UsageFieldsSchema>())).DoNotCallBase();
        manageClient.GetAsync<UsageFieldsSchema, UsageFieldsResponse>(url, Arg.Any<UsageFieldsSchema>()).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetUsageFields(_projectId, getProjectUsageFieldsSchema);

        // Assert
        await manageClient.Received().GetAsync<UsageFieldsSchema, UsageFieldsResponse>(url, Arg.Any<UsageFieldsSchema>());

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
    public async Task GetBalances_Should_Call_GetAsync_Returning_BalancesResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/balances");
        var expectedResponse = new AutoFaker<BalancesResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<BalancesResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<BalancesResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetBalances(_projectId);

        // Assert
        await manageClient.Received().GetAsync<BalancesResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BalancesResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetBalance_Should_Call_GetAsync_Returning_BalanceResponse()
    {
        // Input and Output
        var balanceId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/balances/{balanceId}");
        var expectedResponse = new AutoFaker<BalanceResponse>().Generate();

        // Fake Client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var manageClient = Substitute.For<ManageClient>(_apiKey, _options, null);
        
        // Mock methods
        manageClient.When(x => x.GetAsync<BalanceResponse>(Arg.Any<string>())).DoNotCallBase();
        manageClient.GetAsync<BalanceResponse>(url).Returns(expectedResponse);

        // Act
        var result = await manageClient.GetBalance(_projectId, balanceId);

        // Assert
        await manageClient.Received().GetAsync<BalanceResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BalanceResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
    #endregion
}
