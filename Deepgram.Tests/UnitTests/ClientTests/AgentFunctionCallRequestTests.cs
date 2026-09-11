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
using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Regression tests for #428: FunctionCallRequestResponse dropped the functions[] array and
/// ErrorResponse dropped the code field, so a client bridging the Agent WebSocket could neither
/// answer a function call nor react to CLIENT_MESSAGE_TIMEOUT.
/// </summary>
public class AgentFunctionCallRequestTests
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

    [Test]
    public async Task FunctionCallRequest_Should_Expose_Functions_From_Wire_Payload()
    {
        var client = new Client(_apiKey, _options);

        FunctionCallRequestResponse? received = null;
        await client.Subscribe(new EventHandler<FunctionCallRequestResponse>((_, e) => received = e));

        const string json = "{\"type\":\"FunctionCallRequest\",\"functions\":[" +
            "{\"id\":\"fc_123\",\"name\":\"get_weather\",\"arguments\":\"{\\\"location\\\":\\\"Seattle\\\"}\",\"client_side\":true}," +
            "{\"id\":\"fc_456\",\"name\":\"lookup_order\",\"arguments\":\"{}\",\"client_side\":false}" +
            "]}";

        using (new AssertionScope())
        {
            Action act = () => FeedTextMessage(client, json);
            act.Should().NotThrow();

            received.Should().NotBeNull();
            received!.Type.Should().Be(AgentType.FunctionCallRequest);
            received.Functions.Should().HaveCount(2, "every entry in functions[] must survive deserialization");

            var first = received.Functions![0];
            first.Id.Should().Be("fc_123");
            first.Name.Should().Be("get_weather");
            first.Arguments.Should().Be("{\"location\":\"Seattle\"}", "arguments is a JSON-encoded string on the wire");
            first.ClientSide.Should().BeTrue();

            received.Functions[1].ClientSide.Should().BeFalse();
        }
    }

    [Test]
    public async Task FunctionCallRequest_Without_Functions_Should_Still_Dispatch()
    {
        var client = new Client(_apiKey, _options);

        FunctionCallRequestResponse? received = null;
        await client.Subscribe(new EventHandler<FunctionCallRequestResponse>((_, e) => received = e));

        FeedTextMessage(client, "{\"type\":\"FunctionCallRequest\"}");

        received.Should().NotBeNull();
        received!.Functions.Should().BeNull();
    }

    [Test]
    public async Task Error_Should_Expose_Code_From_Wire_Payload()
    {
        var client = new Client(_apiKey, _options);

        ErrorResponse? received = null;
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => received = e));

        const string json = "{\"type\":\"Error\",\"description\":\"Client did not respond in time\",\"code\":\"CLIENT_MESSAGE_TIMEOUT\"}";

        using (new AssertionScope())
        {
            Action act = () => FeedTextMessage(client, json);
            act.Should().NotThrow();

            received.Should().NotBeNull();
            received!.Code.Should().Be("CLIENT_MESSAGE_TIMEOUT", "code must survive the Common -> Agent ErrorResponse copy");
            received.Description.Should().Be("Client did not respond in time");
        }
    }

    [Test]
    public void FunctionCall_Should_Round_Trip_Through_Serialization()
    {
        var call = new FunctionCall { Id = "fc_1", Name = "f", Arguments = "{\"a\":1}", ClientSide = true };

        var json = call.ToString();
        var back = JsonSerializer.Deserialize<FunctionCall>(json);

        back.Should().BeEquivalentTo(call);
    }
}
