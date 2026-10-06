// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.SelfHosted.v1;
using AuraSpeakSchema = Deepgram.Models.Speak.v1.REST.SpeakSchema;
using AuraTextSource = Deepgram.Models.Speak.v1.REST.TextSource;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Exercises the current self-hosted distribution credentials API against a disposable project.
/// The supplied API credential is intentionally read from the test suite's startup snapshot and
/// never logged.
/// </summary>
public class SelfHostedDistributionCredentialsLiveIntegrationTests
{
    [Test]
    public async Task Live_AuraRest_Should_Accept_Numeric_String_Output_Options()
    {
        var apiKey = GetLiveApiKey();
        var client = ClientFactory.CreateSpeakRESTClient(apiKey);

        var mp3Response = await client.ToStream(new AuraTextSource("Phase zero bitrate validation."), new AuraSpeakSchema
        {
            Model = "aura-2-thalia-en",
            Encoding = "mp3",
            BitRate = "48000",
        });
        var linear16Response = await client.ToStream(new AuraTextSource("Phase zero sample rate validation."), new AuraSpeakSchema
        {
            Model = "aura-2-thalia-en",
            Encoding = "linear16",
            SampleRate = "24000",
        });

        mp3Response.Stream.Should().NotBeNull();
        mp3Response.Stream!.Length.Should().BeGreaterThan(0);
        linear16Response.Stream.Should().NotBeNull();
        linear16Response.Stream!.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Live_SelfHostedDistributionCredentials_Should_List()
    {
        var (apiKey, projectId) = GetLiveConfiguration();
        var client = ClientFactory.CreateSelfHostedDistributionCredentialsClient(apiKey);
        var credentials = await client.ListDistributionCredentials(projectId);

        credentials.Should().NotBeNull();
    }

    [Test]
    public async Task Live_SelfHostedDistributionCredentials_Should_Create_Get_List_And_Delete()
    {
        var (apiKey, projectId) = GetLiveConfiguration();
        var client = ClientFactory.CreateSelfHostedDistributionCredentialsClient(apiKey);
        var comment = $"dotnet-sdk-phase-zero-{Guid.NewGuid():N}";
        string? distributionCredentialsId = null;

        try
        {
            var created = await client.CreateDistributionCredentials(projectId,
                new DistributionCredentialsCreateSchema { Comment = comment },
                new DistributionCredentialsCreateOptions
                {
                    Provider = "quay",
                    Scopes = new List<string> { "self-hosted:products" },
                });

            distributionCredentialsId = created.DistributionCredentials?.DistributionCredentialsId;
            if (string.IsNullOrWhiteSpace(distributionCredentialsId))
            {
                var credentials = await client.ListDistributionCredentials(projectId);
                distributionCredentialsId = credentials.DistributionCredentials?
                    .SingleOrDefault(item => item.DistributionCredentials?.Comment == comment)?
                    .DistributionCredentials?.DistributionCredentialsId;
            }

            distributionCredentialsId.Should().NotBeNullOrWhiteSpace();

            var retrieved = await client.GetDistributionCredentials(projectId, distributionCredentialsId!);
            using (new AssertionScope())
            {
                retrieved.DistributionCredentials?.DistributionCredentialsId.Should().Be(distributionCredentialsId);
                retrieved.DistributionCredentials?.Comment.Should().Be(comment);
                retrieved.DistributionCredentials?.Provider.Should().Be("quay");
                retrieved.DistributionCredentials?.Scopes.Should().Contain("self-hosted:products");
            }

            var listed = await client.ListDistributionCredentials(projectId);
            listed.DistributionCredentials?.Select(item => item.DistributionCredentials?.DistributionCredentialsId)
                .Should().Contain(distributionCredentialsId);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(distributionCredentialsId))
            {
                await client.DeleteDistributionCredentials(projectId, distributionCredentialsId);
            }
        }
    }

    private static (string ApiKey, string ProjectId) GetLiveConfiguration()
    {
        var apiKey = GetLiveApiKey();
        var projectId = Environment.GetEnvironmentVariable("DEEPGRAM_SELF_HOSTED_PROJECT_ID")
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_PROJECT_ID");
        if (string.IsNullOrWhiteSpace(projectId))
        {
            Assert.Ignore("DEEPGRAM_SELF_HOSTED_PROJECT_ID or DEEPGRAM_PROJECT_ID is not set. Skipping live self-hosted credentials test.");
        }

        return (apiKey!, projectId!);
    }

    private static string GetLiveApiKey()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live API test.");
        }

        return apiKey!;
    }
}
