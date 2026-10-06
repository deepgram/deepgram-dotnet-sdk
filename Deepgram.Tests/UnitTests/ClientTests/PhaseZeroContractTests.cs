// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Clients.SelfHosted.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v1.REST;
using Deepgram.Models.SelfHosted.v1;
using Deepgram.Utilities;
using AuraSpeakSchema = Deepgram.Models.Speak.v1.REST.SpeakSchema;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class PhaseZeroContractTests
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

    [TestCase("POST")]
    [TestCase("PUT")]
    public void PreRecorded_CallbackMethod_Addon_Should_Preserve_Http_Method(string callbackMethod)
    {
        var schema = new PreRecordedSchema { CallBack = "https://example.test/callback" };

        var query = HttpUtility.ParseQueryString(new Uri(QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/listen", schema,
            new Dictionary<string, string> { ["callback_method"] = callbackMethod })).Query);

        query["callback_method"].Should().Be(callbackMethod);
    }

    [Test]
    public void AuraRest_Numeric_Options_Should_Use_Current_String_Query_Serialization()
    {
        var schema = new AuraSpeakSchema { SampleRate = "24000", BitRate = "48000" };

        var query = HttpUtility.ParseQueryString(new Uri(QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/speak", schema)).Query);

        query["sample_rate"].Should().Be("24000");
        query["bit_rate"].Should().Be("48000");
    }
}
