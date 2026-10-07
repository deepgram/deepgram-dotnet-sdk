// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Tests.UnitTests.ClientTests;

public class FluxSpeakTextControlsTests
{
    [Test]
    public void Pronunciation_Should_Escape_Json_Content()
    {
        var control = FluxSpeakTextControls.Pronunciation("C#", "siː ʃɑːrp");
        using var document = JsonDocument.Parse(control[1..]);

        document.RootElement.GetProperty("word").GetString().Should().Be("C#");
        document.RootElement.GetProperty("pronounce").GetString().Should().Be("siː ʃɑːrp");
    }

    [Test]
    public void Pause_Should_Format_Rest_Marker()
    {
        FluxSpeakTextControls.Pause(TimeSpan.FromMilliseconds(800)).Should().Be("\\{pause:800ms}");
    }
}
