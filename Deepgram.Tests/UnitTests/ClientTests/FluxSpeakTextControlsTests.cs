// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Tests.UnitTests.ClientTests;

public class FluxSpeakTextControlsTests
{
    [Test]
    public void Pronunciation_Should_Escape_Json_Content()
    {
        var control = FluxSpeakTextControls.Pronunciation("\"C#\\", "kˈɑpiɹˌaɪt");
        control.Should().EndWith("\\}");
        using var document = JsonDocument.Parse(control[1..^2] + "}");

        document.RootElement.GetProperty("word").GetString().Should().Be("\"C#\\");
        document.RootElement.GetProperty("pronounce").GetString().Should().Be("kˈɑpiɹˌaɪt");
    }

    [Test]
    public void Pronunciation_Should_Keep_Ipa_As_Raw_Utf8()
    {
        FluxSpeakTextControls.Pronunciation("dupilumab", "duːˈpɪljuːmæb")
            .Should().Be("\\{\"word\":\"dupilumab\",\"pronounce\":\"duːˈpɪljuːmæb\"\\}");
    }

    [Test]
    public void Pause_Should_Format_Rest_Marker()
    {
        FluxSpeakTextControls.Pause(TimeSpan.FromMilliseconds(500)).Should().Be("\\{pause:500ms\\}");
        FluxSpeakTextControls.Pause(TimeSpan.FromMilliseconds(3000)).Should().Be("\\{pause:3000ms\\}");
    }

    [Test]
    public void Pause_With_Sub_Millisecond_Duration_Should_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FluxSpeakTextControls.Pause(TimeSpan.FromTicks(5_000_001)));
    }

    [TestCase(550)]
    [TestCase(400)]
    [TestCase(3100)]
    public void Pause_With_Unsupported_Duration_Should_Throw(int milliseconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FluxSpeakTextControls.Pause(TimeSpan.FromMilliseconds(milliseconds)));
    }
}
