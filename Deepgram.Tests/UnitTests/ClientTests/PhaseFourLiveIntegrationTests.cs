// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Listen.v1.REST;
using Deepgram.Models.Speak.v1.REST;
using AnalyzeTextSource = Deepgram.Models.Analyze.v1.TextSource;
using ListenUrlSource = Deepgram.Models.Listen.v1.REST.UrlSource;
using SpeakTextSource = Deepgram.Models.Speak.v1.REST.TextSource;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class PhaseFourLiveIntegrationTests
{
    [Test]
    public async Task Live_Current_Request_Options_Should_Be_Accepted()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping Phase 4 live integration test.");
        }

        var speak = ClientFactory.CreateSpeakRESTClient(apiKey!);
        var audio = await speak.ToStream(new SpeakTextSource("Phase four integration check."), new SpeakSchema
        {
            Model = "aura-2-thalia-en",
            Speed = 1.1,
            Tag = new List<string> { "phase-four" },
            MipOptOut = true,
        });

        var listen = ClientFactory.CreateListenRESTClient(apiKey);
        var transcript = await listen.TranscribeUrl(new ListenUrlSource("https://dpgr.am/bueller.wav"), new PreRecordedSchema
        {
            Model = "nova-3",
            MipOptOut = true,
        });

        var analyze = ClientFactory.CreateAnalyzeClient(apiKey);
        var analysis = await analyze.AnalyzeText(new AnalyzeTextSource("Phase four integration check."), new AnalyzeSchema
        {
            Language = "en",
            Topics = true,
            Tag = new List<string> { "phase-four" },
        });

        using (new AssertionScope())
        {
            audio.Stream.Should().NotBeNull();
            audio.Stream!.Length.Should().BeGreaterThan(0);
            transcript.Results.Should().NotBeNull();
            analysis.Results.Should().NotBeNull();
        }
    }
}
