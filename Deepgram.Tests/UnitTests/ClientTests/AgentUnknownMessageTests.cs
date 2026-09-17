// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Net.WebSockets;
using System.Text;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Agent.v2.WebSocket;
using Deepgram.Models.Common.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Regression tests for #395: the Agent client parsed the message "type" with Enum.Parse, which
/// threw ArgumentException for any message type unknown to this SDK version (e.g. the server's
/// "History" and "Warning" messages). The client must instead route unknown types to the Unhandled
/// event without throwing, keeping it forward-compatible with new server message types.
/// </summary>
public class AgentUnknownMessageTests
{
    private DeepgramWsClientOptions _options = null!;
    private string _apiKey = null!;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey) { OnPrem = true };
    }

    private static void FeedTextMessage(Client client, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var ms = new MemoryStream(bytes);
        var result = new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true);
        client.ProcessTextMessage(result, ms);
    }

    [TestCase("Warning")]
    [TestCase("History")]
    public async Task ProcessTextMessage_With_Unknown_Type_Should_Not_Throw_And_Surface_As_Unhandled(string unknownType)
    {
        var client = new Client(_apiKey, _options);

        UnhandledResponse? unhandled = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((_, e) => unhandled = e));

        var json = $"{{\"type\":\"{unknownType}\",\"description\":\"something new from the server\"}}";

        using (new AssertionScope())
        {
            // Before the fix this threw ArgumentException ("Requested value 'Warning' was not found.")
            Action act = () => FeedTextMessage(client, json);
            act.Should().NotThrow($"unknown Agent message type '{unknownType}' must not crash the receive loop");

            // InvokeParallel dispatches synchronously (Parallel.ForEach), so the handler has already run.
            unhandled.Should().NotBeNull($"unknown type '{unknownType}' must be surfaced via the Unhandled event");
            unhandled!.Type.Should().Be(WebSocketType.Unhandled);
            unhandled.Raw.Should().Contain(unknownType, "the raw payload must be preserved for the caller to inspect");
        }
    }

    [Test]
    public async Task ProcessTextMessage_With_Known_Type_Should_Still_Dispatch()
    {
        var client = new Client(_apiKey, _options);

        Deepgram.Models.Agent.v2.WebSocket.WelcomeResponse? welcome = null;
        await client.Subscribe(new EventHandler<Deepgram.Models.Agent.v2.WebSocket.WelcomeResponse>((_, e) => welcome = e));

        var json = "{\"type\":\"Welcome\",\"request_id\":\"abc-123\"}";

        using (new AssertionScope())
        {
            Action act = () => FeedTextMessage(client, json);
            act.Should().NotThrow();
            welcome.Should().NotBeNull("a known type must still be routed to its typed event after the TryParse change");
        }
    }
}
