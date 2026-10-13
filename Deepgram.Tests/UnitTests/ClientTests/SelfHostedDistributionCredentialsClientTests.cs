// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Clients.SelfHosted.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.SelfHosted.v1;
using Deepgram.Utilities;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SelfHostedDistributionCredentialsClientTests
{
    private DeepgramHttpClientOptions _options = null!;
    private string _apiKey = null!;
    private string _projectId = null!;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey) { OnPrem = true };
        _projectId = new Faker().Random.Guid().ToString();
    }

    [Test]
    public async Task CreateDistributionCredentials_Should_Use_Current_Route_And_Separate_Query_Options()
    {
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        var request = new DistributionCredentialsCreateSchema { Comment = "staging cluster" };
        var options = new DistributionCredentialsCreateOptions
        {
            Provider = "quay",
            Scopes = new List<string> { "self-hosted:product:api", "self-hosted:product:engine" },
        };
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            Arg.Any<string>(), Arg.Any<DistributionCredentialsCreateOptions>(), Arg.Any<DistributionCredentialsCreateSchema>()))
            .DoNotCallBase();
        client.PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(uri, options, request)
            .Returns(expectedResponse);

        var result = await client.CreateDistributionCredentials(_projectId, request, options);

        await client.Received().PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            uri, options, request);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task CreateDistributionCredentials_Should_Use_An_Empty_Body_When_Omitted()
    {
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            Arg.Any<string>(), null, Arg.Any<DistributionCredentialsCreateSchema>())).DoNotCallBase();
        client.PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            uri, null, Arg.Any<DistributionCredentialsCreateSchema>()).Returns(expectedResponse);

        var result = await client.CreateDistributionCredentials(_projectId);

        await client.Received().PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            uri, null, Arg.Is<DistributionCredentialsCreateSchema>(schema => schema.Comment == null));
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public void DistributionCredentialsClient_Should_Suppress_Response_Body_Logging()
    {
        var client = new InspectableDistributionCredentialsClient(_apiKey, _options);

        client.LogResponseBodiesForTest.Should().BeFalse();
    }

    [Test]
    public async Task ListDistributionCredentials_Should_Use_Current_Route()
    {
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        var expectedResponse = new AutoFaker<CredentialsResponse>().Generate();
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.GetAsync<CredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        client.GetAsync<CredentialsResponse>(uri).Returns(expectedResponse);

        var result = await client.ListDistributionCredentials(_projectId);

        await client.Received().GetAsync<CredentialsResponse>(uri);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task GetDistributionCredentials_Should_Use_Current_Route_And_Credential_Id()
    {
        var distributionCredentialsId = new Faker().Random.Guid().ToString();
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}/{distributionCredentialsId}");
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.GetAsync<CredentialResponse>(Arg.Any<string>())).DoNotCallBase();
        client.GetAsync<CredentialResponse>(uri).Returns(expectedResponse);

        var result = await client.GetDistributionCredentials(_projectId, distributionCredentialsId);

        await client.Received().GetAsync<CredentialResponse>(uri);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task DeleteDistributionCredentials_Should_Use_Current_Route_And_Credential_Id()
    {
        var distributionCredentialsId = new Faker().Random.Guid().ToString();
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}/{distributionCredentialsId}");
        var expectedResponse = new AutoFaker<CredentialResponse>().Generate();
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.DeleteAsync<CredentialResponse>(Arg.Any<string>())).DoNotCallBase();
        client.DeleteAsync<CredentialResponse>(uri).Returns(expectedResponse);

        var result = await client.DeleteDistributionCredentials(_projectId, distributionCredentialsId);

        await client.Received().DeleteAsync<CredentialResponse>(uri);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public void CreateDistributionCredentials_Should_Serialize_Scopes_And_Provider_As_Query_Parameters()
    {
        var uri = AbstractRestClient.GetUri(_options,
            $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        var options = new DistributionCredentialsCreateOptions
        {
            Provider = "quay",
            Scopes = new List<string> { "self-hosted:product:api", "self-hosted:product:engine" },
        };

        var query = HttpUtility.ParseQueryString(new Uri(QueryParameterUtil.FormatURL(uri, options)).Query);

        query["provider"].Should().Be("quay");
        query.GetValues("scopes").Should().BeEquivalentTo("self-hosted:product:api", "self-hosted:product:engine");
    }

    [Test]
    public void CreateDistributionCredentials_Should_Serialize_Only_Comment_In_Body()
    {
        var request = new DistributionCredentialsCreateSchema { Comment = "staging cluster" };

        var payload = JsonSerializer.Serialize(request, JsonSerializeOptions.DefaultOptions);
        var document = JsonDocument.Parse(payload);

        document.RootElement.EnumerateObject().Select(property => property.Name).Should().Equal("comment");
        document.RootElement.GetProperty("comment").GetString().Should().Be("staging cluster");
    }

    [Test]
    public async Task ListAndGetDistributionCredentials_Should_Call_The_Literal_SelfHosted_Url()
    {
        var distributionCredentialsId = new Faker().Random.Guid().ToString();
        var listUrl = $"https://api.deepgram.com/v1/projects/{_projectId}/self-hosted/distribution/credentials";
        var getUrl = $"{listUrl}/{distributionCredentialsId}";
        var client = Substitute.For<DistributionCredentialsClient>(_apiKey, _options, null);

        client.When(x => x.GetAsync<CredentialsResponse>(Arg.Any<string>())).DoNotCallBase();
        client.When(x => x.GetAsync<CredentialResponse>(Arg.Any<string>())).DoNotCallBase();
        client.GetAsync<CredentialsResponse>(Arg.Any<string>()).Returns(new AutoFaker<CredentialsResponse>().Generate());
        client.GetAsync<CredentialResponse>(Arg.Any<string>()).Returns(new AutoFaker<CredentialResponse>().Generate());

        await client.ListDistributionCredentials(_projectId);
        await client.GetDistributionCredentials(_projectId, distributionCredentialsId);

        await client.Received(1).GetAsync<CredentialsResponse>(listUrl);
        await client.Received(1).GetAsync<CredentialResponse>(getUrl);
    }

    private sealed class InspectableDistributionCredentialsClient : DistributionCredentialsClient
    {
        public InspectableDistributionCredentialsClient(string apiKey, DeepgramHttpClientOptions options)
            : base(apiKey, options)
        {
        }

        public bool LogResponseBodiesForTest => LogResponseBodies;
    }
}
