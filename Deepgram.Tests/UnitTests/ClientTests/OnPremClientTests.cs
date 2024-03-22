// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.OnPrem.v1;
using Deepgram.Clients.OnPrem.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class OnPremClientTests
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

    [Test]
    public async Task ListCredentials_Should_Call_GetAsync_Returning_CredentialsResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}");
        var expectedResponse = new AutoFaker<CredentialsResponse>().Generate();

        // Fake client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);

        // Mock methods
        onPremClient.When(x => x.GetAsync<CredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<CredentialsResponse>(url).Returns(expectedResponse);

        // Act
        var result = await onPremClient.ListCredentials(_projectId);

        // Assert
        await onPremClient.Received().GetAsync<CredentialsResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CredentialsResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task GetCredentials_Should_Call_GetAsync_Returning_CredentialResponse()
    {
        // Input and Output
        var credentialsId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}");
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();

        // Fake client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        
        // Mock methods
        onPremClient.When(x => x.GetAsync<CredentialResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.GetAsync<CredentialResponse>(url).Returns(expectedResponse);

        // Act
        var result = await onPremClient.GetCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().GetAsync<CredentialResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CredentialResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteCredentials_Should_Call_DeleteAsync_Returning_MessageResponse()
    {
        // Input and Output
        var credentialsId = new Faker().Random.Guid().ToString();
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}/{credentialsId}");
        var expectedResponse = new AutoFaker<MessageResponse>().Generate();
        
        // Fake client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        
        // Mock methods
        onPremClient.When(x => x.DeleteAsync<MessageResponse>(Arg.Any<string>())).DoNotCallBase();
        onPremClient.DeleteAsync<MessageResponse>(url).Returns(expectedResponse);

        // Act
        var result = await onPremClient.DeleteCredentials(_projectId, credentialsId);

        // Assert
        await onPremClient.Received().DeleteAsync<MessageResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<MessageResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }


    [Test]
    public async Task CreateCredentials_Should_Return_CredentialResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.ONPREM}");
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();
        var createOnPremCredentialsSchema = new CredentialsSchema();

        // Fake client
        var httpClient = MockHttpClient.CreateHttpClientWithResult(expectedResponse);
        var onPremClient = Substitute.For<OnPremClient>(_apiKey, _options);
        
        // Mock methods
        onPremClient.When(x => x.PostAsync<CredentialsSchema, CredentialResponse>(Arg.Any<string>(), Arg.Any<CredentialsSchema>())).DoNotCallBase();
        onPremClient.PostAsync<CredentialsSchema, CredentialResponse>(url, Arg.Any<CredentialsSchema>()).Returns(expectedResponse);

        // Act
        var result = await onPremClient.CreateCredentials(_projectId, createOnPremCredentialsSchema);

        // Assert
        await onPremClient.Received().PostAsync<CredentialsSchema, CredentialResponse>(url, Arg.Any<CredentialsSchema>());

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<CredentialResponse>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
