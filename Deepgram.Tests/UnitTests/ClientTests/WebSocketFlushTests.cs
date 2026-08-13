// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using ListenV2 = Deepgram.Clients.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class WebSocketFlushTests
{
    [Test]
    public void CreateFlushMarker_Should_Carry_The_Signal_And_No_Payload()
    {
        var tcs = new TaskCompletionSource<bool>();

        var marker = WebSocketMessage.CreateFlushMarker(tcs);

        using (new AssertionScope())
        {
            ((object?)marker.FlushSignal).Should().BeSameAs(tcs);
            marker.Length.Should().Be(0);
        }
    }

    [Test]
    public void Normal_Message_Should_Not_Be_A_Flush_Marker()
    {
        var message = new WebSocketMessage(new byte[] { 1, 2, 3 }, System.Net.WebSockets.WebSocketMessageType.Binary);

        using (new AssertionScope())
        {
            ((object?)message.FlushSignal).Should().BeNull();
            message.Length.Should().Be(3);
        }
    }

    [Test]
    public async Task Flush_Should_Return_Immediately_When_Not_Connected()
    {
        var client = new ListenV2.Client("fake-key");

        // Not connected: Flush must not enqueue-and-await forever; it returns right away.
        var flush = client.Flush();
        var completed = await Task.WhenAny(flush, Task.Delay(TimeSpan.FromSeconds(2)));

        completed.Should().BeSameAs(flush, "Flush should short-circuit when the socket is not connected");
        await flush; // observe any exception
    }
}
