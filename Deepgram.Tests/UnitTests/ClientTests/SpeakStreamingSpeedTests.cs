// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;

using Deepgram.Clients.Speak.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SpeakStreamingSpeedTests
{
    private string _apiKey = null!;
    private DeepgramWsClientOptions _options = null!;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey) { OnPrem = true };
    }

    [Test]
    public void GetUri_Should_Send_Speed_On_Streaming()
    {
        var uri = Client.GetUri(_options, new SpeakSchema { Model = "aura-2-thalia-en", Speed = 1.2 });

        uri.Query.Should().Contain("speed=1.2");
    }

    [Test]
    public void GetUri_Should_Omit_Speed_When_Unset()
    {
        var uri = Client.GetUri(_options, new SpeakSchema { Model = "aura-2-thalia-en" });

        uri.Query.Should().NotContain("speed");
    }

    [Test]
    public void GetUri_Should_Send_Speed_Of_One()
    {
        // 1.0 is the default, but an explicitly set value must still reach the wire
        // rather than being dropped as if it were unset.
        var uri = Client.GetUri(_options, new SpeakSchema { Model = "aura-2-thalia-en", Speed = 1.0 });

        uri.Query.Should().Contain("speed=1");
    }

    [Test]
    public void GetUri_Should_Format_Speed_With_Invariant_Culture()
    {
        // Regression: on a comma-decimal culture a culture-sensitive ToString() would
        // produce "0,7" and the API would reject the parameter.
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var uri = Client.GetUri(_options, new SpeakSchema { Model = "aura-2-thalia-en", Speed = 0.7 });

            using (new AssertionScope())
            {
                uri.Query.Should().Contain("speed=0.7");
                uri.Query.Should().NotContain("0%2c7");
                uri.Query.Should().NotContain("0,7");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void SpeakSchema_Should_Round_Trip_Speed()
    {
        var json = new SpeakSchema { Model = "aura-2-thalia-en", Speed = 1.5 }.ToString();

        var restored = JsonSerializer.Deserialize<SpeakSchema>(json);

        restored!.Speed.Should().Be(1.5);
    }

    [Test]
    public void SpeakSchema_Should_Omit_Speed_From_Serialized_Payload_When_Unset()
    {
        var json = new SpeakSchema { Model = "aura-2-thalia-en" }.ToString();

        json.Should().NotContain("speed");
    }
}
