// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using ListenV1 = Deepgram.Clients.Listen.v1.WebSocket;
using Deepgram.Models.Listen.v1.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenV1ConfigureLiveIntegrationTests
{
    [Test]
    public async Task Live_ListenV1_Configure_Keyterms_Should_Not_Return_Error()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping Listen v1 Configure integration test.");
        }

        var client = new ListenV1.Client(apiKey);
        ErrorResponse? error = null;
        client.Subscribe(new EventHandler<ErrorResponse>((_, response) => error = response));

        try
        {
            await client.Connect(new LiveSchema { Model = "nova-3", InterimResults = true });
            client.SendConfigure(new ConfigureSchema { Keyterms = new List<string> { "Deepgram" } });
            await Task.Delay(TimeSpan.FromSeconds(1));

            error.Should().BeNull();
        }
        finally
        {
            await client.Stop();
        }
    }
}
