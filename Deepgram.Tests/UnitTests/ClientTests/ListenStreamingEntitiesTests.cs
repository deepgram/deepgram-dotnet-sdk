// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;

using Deepgram.Clients.Listen.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenStreamingEntitiesTests
{
    private string _apiKey = null!;
    private DeepgramWsClientOptions _options = null!;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey) { OnPrem = true };
    }

    private static void FeedTextMessage(Client client, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        client.ProcessTextMessage(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true), stream);
    }

    [Test]
    public void GetUri_Should_Send_DetectEntities_On_Streaming()
    {
        var uri = Client.GetUri(_options, new LiveSchema { Model = "nova-3", DetectEntities = true });

        uri.Query.Should().Contain("detect_entities=true");
    }

    [Test]
    public void GetUri_Should_Send_DiarizeModel_On_Streaming()
    {
        var uri = Client.GetUri(_options, new LiveSchema { Model = "nova-3", DiarizeModel = "latest" });

        uri.Query.Should().Contain("diarize_model=latest");
    }

    [Test]
    public void GetUri_Should_Omit_New_Parameters_When_Unset()
    {
        var uri = Client.GetUri(_options, new LiveSchema { Model = "nova-3" });

        using (new AssertionScope())
        {
            uri.Query.Should().NotContain("detect_entities");
            uri.Query.Should().NotContain("diarize_model");
        }
    }

    [Test]
    public void GetUri_Should_Keep_DiarizeModel_Independent_Of_Legacy_DiarizeVersion()
    {
        // diarize_model supersedes diarize_version, but setting one must not rewrite the other.
        var uri = Client.GetUri(_options, new LiveSchema
        {
            Model = "nova-3",
            DiarizeModel = "v1",
            DiarizeVersion = "2021-07-14.0",
        });

        using (new AssertionScope())
        {
            uri.Query.Should().Contain("diarize_model=v1");
            uri.Query.Should().Contain("diarize_version=2021-07-14.0");
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Surface_Entities_On_Final_Results()
    {
        var client = new Client(_apiKey, _options);
        ResultResponse? received = null;
        await client.Subscribe(new EventHandler<ResultResponse>((_, response) => received = response));

        FeedTextMessage(client, """
        {
          "type": "Results",
          "is_final": true,
          "channel": { "alternatives": [ { "transcript": "call 555 0100" } ] },
          "entities": [
            {
              "label": "PHONE_NUMBER",
              "value": "555-0100",
              "raw_value": "five five five oh one oh oh",
              "confidence": 0.97,
              "start_word": 1,
              "end_word": 5
            }
          ]
        }
        """);

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Entities.Should().HaveCount(1);
            var entity = received.Entities![0];
            entity.Label.Should().Be("PHONE_NUMBER");
            entity.Value.Should().Be("555-0100");
            entity.Confidence.Should().Be(0.97);
            entity.StartWord.Should().Be(1);
            entity.EndWord.Should().Be(5);
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Preserve_Entity_RawValue()
    {
        // Regression: raw_value carries the original spoken form and must not be
        // collapsed into value when formatting is enabled.
        var client = new Client(_apiKey, _options);
        ResultResponse? received = null;
        await client.Subscribe(new EventHandler<ResultResponse>((_, response) => received = response));

        FeedTextMessage(client, """
        {
          "type": "Results",
          "is_final": true,
          "entities": [ { "label": "CARDINAL", "value": "42", "raw_value": "forty two" } ]
        }
        """);

        received!.Entities![0].RawValue.Should().Be("forty two");
        received.Entities[0].Value.Should().Be("42");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Treat_Empty_Entities_As_Empty_List()
    {
        var client = new Client(_apiKey, _options);
        ResultResponse? received = null;
        await client.Subscribe(new EventHandler<ResultResponse>((_, response) => received = response));

        FeedTextMessage(client, """{ "type": "Results", "is_final": true, "entities": [] }""");

        received!.Entities.Should().NotBeNull();
        received.Entities.Should().BeEmpty();
    }

    [Test]
    public async Task ProcessTextMessage_Should_Leave_Entities_Null_When_Absent()
    {
        // Back-compat: a Results frame without detect_entities enabled carries no
        // entities key at all, and must not start materialising an empty list.
        var client = new Client(_apiKey, _options);
        ResultResponse? received = null;
        await client.Subscribe(new EventHandler<ResultResponse>((_, response) => received = response));

        FeedTextMessage(client, """{ "type": "Results", "is_final": true, "start": 0.0 }""");

        received.Should().NotBeNull();
        received!.Entities.Should().BeNull();
    }

    [Test]
    public async Task ProcessTextMessage_Should_Ignore_Unknown_Entity_Fields()
    {
        // A new server-side entity field must stay non-fatal for a deployed client.
        var client = new Client(_apiKey, _options);
        ResultResponse? received = null;
        await client.Subscribe(new EventHandler<ResultResponse>((_, response) => received = response));

        FeedTextMessage(client, """
        {
          "type": "Results",
          "is_final": true,
          "entities": [ { "label": "NAME", "value": "Ada", "some_future_field": true } ]
        }
        """);

        received!.Entities.Should().HaveCount(1);
        received.Entities![0].Label.Should().Be("NAME");
    }

    [Test]
    public void Entity_Should_Omit_Unset_Properties_When_Serialized()
    {
        var json = new Entity { Label = "NAME", Value = "Ada" }.ToString();

        using (new AssertionScope())
        {
            json.Should().Contain("\"label\"").And.Contain("NAME");
            json.Should().Contain("\"value\"").And.Contain("Ada");
            json.Should().NotContain("raw_value");
            json.Should().NotContain("start_word");
            json.Should().NotContain("end_word");
            json.Should().NotContain("confidence");
        }
    }
}
