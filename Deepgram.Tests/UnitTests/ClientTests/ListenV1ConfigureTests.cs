// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Listen.v1.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenV1ConfigureTests
{
    [Test]
    public void SendConfigure_Should_Serialize_Nullable_Keyterms_And_Features()
    {
        var client = Substitute.For<IListenWebSocketClient>();
        byte[]? sent = null;
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>()))
            .Do(call => sent = call.Arg<byte[]>());

        client.SendConfigure(new ConfigureSchema
        {
            Keyterms = null,
            Features = new Dictionary<string, bool> { ["numerals"] = true },
        });

        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(sent!));
        json.RootElement.GetProperty("type").GetString().Should().Be("Configure");
        json.RootElement.TryGetProperty("keyterms", out _).Should().BeFalse();
        json.RootElement.GetProperty("features").GetProperty("numerals").GetBoolean().Should().BeTrue();
    }

    [Test]
    public void ErrorResponse_Should_Deserialize_Configure_Rejection_Code()
    {
        var error = JsonSerializer.Deserialize<ErrorResponse>("""{"type":"Error","variant":"InvalidConfigureMessage","code":"KeytermsNotSupported","description":"unsupported"}""");

        error!.Variant.Should().Be("InvalidConfigureMessage");
        error.Code.Should().Be("KeytermsNotSupported");
    }
}
