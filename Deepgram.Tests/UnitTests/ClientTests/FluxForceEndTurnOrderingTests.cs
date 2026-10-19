// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.WebSocket;
using FluxConstants = Deepgram.Clients.Flux.WebSocket.Constants;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Deterministic transport-ordering regression test for SendForceEndTurn (PR #426 review B1).
///
/// Send() enqueues binary audio for a background sender, while SendForceEndTurn() writes its
/// control frame via SendMessageImmediately(). Without an explicit flush, the immediate text
/// frame can acquire the socket send mutex before earlier audio has drained, so Flux ends the
/// turn before receiving the final chunks. The fix awaits Flush() first, which guarantees every
/// audio frame queued before the call is written to the socket before {"type":"ForceEndTurn"}.
///
/// This test proves the guarantee on a real WebSocket wire: it blocks the audio send by holding
/// the client's send mutex, queues audio, calls SendForceEndTurn(), verifies nothing overtakes
/// while blocked, then releases the mutex and asserts the server received every audio frame
/// BEFORE the ForceEndTurn text frame. Against the unfixed client this fails: the control frame
/// enters the mutex queue directly and overtakes the still-queued audio chunks.
/// </summary>
public class FluxForceEndTurnOrderingTests
{
    // Exposes the protected send mutex so the test can deterministically block socket writes.
    // No behavior is overridden.
    private sealed class TestFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        public TestFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public SemaphoreSlim SendMutex => _mutexSend;
    }

    [Test]
    public async Task SendForceEndTurn_Should_Not_Overtake_Queued_Audio()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, $"ws://127.0.0.1:{server.Port}", keepAlive: false, onPrem: true);
        var client = new TestFluxClient(apiKey, options);

        try
        {
            var connected = await client.Connect(new FluxSchema
            {
                Model = "flux-general-en",
                Encoding = "linear16",
                SampleRate = 16000,
            });
            connected.Should().BeTrue("the in-process WebSocket server must accept the connection");

            // Block the audio send: the background sender dequeues a message, then waits on the
            // send mutex before writing it to the socket. Holding the mutex freezes the wire.
            (await client.SendMutex.WaitAsync(TimeSpan.FromSeconds(5))).Should().BeTrue();
            var mutexHeld = true;
            try
            {
                // Queue three audio chunks. The sender dequeues the first and blocks on the held
                // mutex; the remaining two stay in the send queue behind it.
                byte[][] chunks = { new byte[] { 0x01, 0x01 }, new byte[] { 0x02, 0x02 }, new byte[] { 0x03, 0x03 } };
                foreach (var chunk in chunks)
                {
                    client.Send(chunk);
                }

                // Give the background sender a moment to pick up the first chunk and block on
                // the held mutex. The correctness guarantee below does not depend on this
                // settling — the flush marker is enqueued behind ALL queued audio either way —
                // it only makes the blocked-wire assertions exercise the deepest interleaving.
                await Task.Delay(200);

                // Now request the manual end of turn while the audio send is blocked.
                var forceEndTurnTask = client.SendForceEndTurn();

                // While audio is blocked, the control frame must not reach the wire: nothing at
                // all may have been written, and SendForceEndTurn must still be waiting.
                await Task.Delay(250);
                forceEndTurnTask.IsCompleted.Should().BeFalse(
                    "SendForceEndTurn must wait for queued audio to flush before sending the control frame");
                server.DataFrames.Should().BeEmpty("no frame can reach the wire while the send mutex is held");

                // Unblock the wire and let everything drain.
                mutexHeld = false;
                client.SendMutex.Release();

                (await Task.WhenAny(forceEndTurnTask, Task.Delay(TimeSpan.FromSeconds(10)))).Should().BeSameAs(forceEndTurnTask,
                    "SendForceEndTurn must complete once the queue has drained");
                await forceEndTurnTask;

                // The server must have received all three audio frames strictly before the
                // ForceEndTurn text frame.
                await WaitUntilAsync(() => server.DataFrames.Count >= 4, TimeSpan.FromSeconds(10),
                    "the server should receive the three audio frames and the control frame");

                var frames = server.DataFrames.ToArray();
                using (new AssertionScope())
                {
                    frames.Length.Should().Be(4);
                    for (var i = 0; i < chunks.Length; i++)
                    {
                        frames[i].IsText.Should().BeFalse($"frame {i} must be binary audio, not a control message");
                        frames[i].Payload.Should().Equal(chunks[i], $"audio chunk {i} must arrive in send order");
                    }
                    frames[3].IsText.Should().BeTrue("the control frame must be the last frame on the wire");
                    var controlJson = Encoding.UTF8.GetString(frames[3].Payload);
                    using var doc = JsonDocument.Parse(controlJson);
                    doc.RootElement.GetProperty("type").GetString().Should().Be(FluxConstants.ForceEndTurn);
                }
            }
            finally
            {
                if (mutexHeld)
                {
                    client.SendMutex.Release();
                }
            }
        }
        finally
        {
            // Close the server side first so the client teardown does not wait out the
            // CloseStream grace period against a server that never answers.
            server.Dispose();
            try
            {
                await client.Stop();
            }
            catch
            {
                // The connection was torn down by the server close above; nothing to clean up.
            }
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout, string because)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }
            await Task.Delay(20);
        }
        condition().Should().BeTrue(because);
    }
}
