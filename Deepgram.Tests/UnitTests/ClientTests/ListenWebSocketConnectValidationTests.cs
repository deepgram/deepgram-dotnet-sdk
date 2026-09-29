// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Listen.v2.WebSocket;
using ListenV2 = Deepgram.Clients.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenWebSocketConnectValidationTests
{
    // Nothing listens on this address, so a Connect() that passes validation fails at the network
    // layer. The tests only care whether validation itself misbehaves.
    private static ListenV2.Client NewClient()
    {
        var options = new DeepgramWsClientOptions("fake-key") { OnPrem = true, BaseAddress = "127.0.0.1:1" };
        return new ListenV2.Client("fake-key", options);
    }

    [Test]
    public async Task Connect_Without_A_Model_Should_Not_Throw_NullReferenceException()
    {
        var client = NewClient();

        Func<Task> act = async () => await client.Connect(new LiveSchema { Language = "en" });

        await act.Should().NotThrowAsync<NullReferenceException>(
            "Model is optional, so validation must not dereference it");
    }

    [Test]
    public async Task Connect_With_Keyterm_And_Without_A_Model_Should_Not_Throw_NullReferenceException()
    {
        var client = NewClient();

        Func<Task> act = async () => await client.Connect(new LiveSchema { Keyterm = new List<string> { "Deepgram" } });

        await act.Should().NotThrowAsync<NullReferenceException>(
            "the keyterm check must skip an unset model instead of dereferencing it");
    }

    [Test]
    public async Task Connect_With_Keyterm_And_A_Non_Nova3_Model_Should_Throw_DeepgramException()
    {
        var client = NewClient();

        Func<Task> act = async () => await client.Connect(new LiveSchema
        {
            Model = "nova-2",
            Keyterm = new List<string> { "Deepgram" },
        });

        await act.Should().ThrowAsync<DeepgramException>().WithMessage("*Nova 3*");
    }
}
