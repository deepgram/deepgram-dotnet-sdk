// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using System.Text.Json.Nodes;

using SpeakTextSourceV1 = Deepgram.Models.Speak.v1.WebSocket.TextSource;
using SpeakTextSourceV2 = Deepgram.Models.Speak.v2.WebSocket.TextSource;
using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Regression tests for the payload-serialization bugs where model ToString() wrapped
/// JsonSerializer.Serialize(...) in Regex.Unescape(...). That corrupted any payload whose
/// text contained quotation marks or backslashes, producing invalid JSON on the wire and
/// causing the server to close the socket (#355) or the client to throw while parsing its
/// own settings (#393). ToString() must now always emit valid, round-trippable JSON.
/// </summary>
public class WebSocketSerializationTests
{
    // Text that previously broke the wire payload: embedded quotes, backslashes, unicode, '+', '&'.
    private const string NastyText = "She said \"hello\" to the CEO & left. Path: C:\\temp\\file. 1+1=2. café ☕";

    [Test]
    public void SpeakV1_TextSource_ToString_Should_Be_Valid_Json_And_RoundTrip()
    {
        var source = new SpeakTextSourceV1(NastyText);

        var json = source.ToString();

        using (new AssertionScope())
        {
            // The bug produced invalid JSON here; Parse would throw.
            Action parse = () => JsonDocument.Parse(json);
            parse.Should().NotThrow("ToString() must emit valid JSON even when text contains quotes/backslashes");

            var roundTripped = JsonSerializer.Deserialize<SpeakTextSourceV1>(json);
            roundTripped!.Text.Should().Be(NastyText, "the exact text must survive a serialize/deserialize round-trip");
            roundTripped.Type.Should().Be("Speak");
        }
    }

    [Test]
    public void SpeakV2_TextSource_ToString_Should_Be_Valid_Json_And_RoundTrip()
    {
        var source = new SpeakTextSourceV2(NastyText);

        var json = source.ToString();

        using (new AssertionScope())
        {
            Action parse = () => JsonDocument.Parse(json);
            parse.Should().NotThrow("ToString() must emit valid JSON even when text contains quotes/backslashes");

            var roundTripped = JsonSerializer.Deserialize<SpeakTextSourceV2>(json);
            roundTripped!.Text.Should().Be(NastyText);
        }
    }

    [Test]
    public void Agent_UpdatePromptSchema_ToString_Should_Be_Parseable_By_JsonNode()
    {
        // Mirrors the Agent Connect path (#393): options.ToString() is fed straight into
        // JsonNode.Parse(...). A prompt with quotes previously made that Parse throw.
        var schema = new UpdatePromptSchema
        {
            Prompt = "You are \"Alex\". Reply in JSON like {\"ok\": true}. Use C:\\path when asked."
        };

        var json = schema.ToString();

        using (new AssertionScope())
        {
            Action parse = () => JsonNode.Parse(json);
            parse.Should().NotThrow("the Agent Connect path parses settings JSON with JsonNode.Parse");

            var roundTripped = JsonSerializer.Deserialize<UpdatePromptSchema>(json);
            roundTripped!.Prompt.Should().Be(schema.Prompt, "the prompt text must round-trip exactly");
        }
    }

    [Test]
    public void Plain_Ascii_Text_Should_Not_Be_Escaped()
    {
        // The relaxed encoder should keep readable characters literal (no \u002B / \u0026 noise).
        var source = new SpeakTextSourceV2("a+b & c < d > e");

        var json = source.ToString();

        using (new AssertionScope())
        {
            json.Should().Contain("a+b & c < d > e", "the relaxed encoder must emit these characters literally");
            json.Should().NotContain("\\u", "no unicode-escaped characters should appear for simple ASCII text");
        }
    }
}
