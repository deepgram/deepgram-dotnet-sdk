// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SpeakWebSocketAutoFlushTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task SpeakClient_AutoFlush_Should_Send_Flush_And_Leave_The_Client_Usable()
    {
        using var server = new FlushCountingWebSocketServer();

        var options = new DeepgramWsClientOptions(
            apiKey: "test-api-key",
            baseAddress: $"ws://127.0.0.1:{server.Port}/v1",
            onPrem: true)
        {
            AutoFlushSpeakDelta = 1,
        };
        var client = new SpeakWebSocketClient("test-api-key", options);

        try
        {
            (await client.Connect(new SpeakSchema())).Should().BeTrue();

            client.SpeakWithText("Hello from the autoflush test.");

            // The autoflush thread must put a Flush frame on the wire.
            await server.FirstFlush.WaitAsync(WaitTimeout);

            // A user Flush after autoflush must not block on the autoflush lock.
            await Task.Run(() => client.Flush()).WaitAsync(WaitTimeout);
            await server.SecondFlush.WaitAsync(WaitTimeout);
        }
        finally
        {
            await client.Stop();
        }
    }

    [Test]
    public void SpeakClient_SpeakWithText_Should_Enqueue_The_Speak_While_Holding_The_AutoFlush_Lock()
    {
        var options = new DeepgramWsClientOptions(apiKey: "test-api-key", onPrem: true)
        {
            AutoFlushSpeakDelta = 1,
        };
        var client = new SpeakWebSocketClient("test-api-key", options);
        bool? lockHeldAtSeam = null;
        string? queuedType = null;

        // The seam runs after the enqueue, still inside the lock. An autoflush tick decides and
        // enqueues its Flush under that same lock, so if the Speak is queued before the lock is
        // released, no Flush can be queued ahead of it. The client is not connected, so nothing
        // drains the send channel and the Speak is still at its head.
        client.OnSpeakEnqueued = () =>
        {
            lockHeldAtSeam = client._mutexLastDatagram.CurrentCount == 0;
            if (client._sendChannel.Reader.TryPeek(out var queued))
            {
                using var json = JsonDocument.Parse(queued.Message.AsMemory());
                queuedType = json.RootElement.GetProperty("type").GetString();
            }
        };

        client.SpeakWithText("Hello from the ordering test.");

        lockHeldAtSeam.Should().BeTrue("the Speak must be enqueued before _mutexLastDatagram is released");
        queuedType.Should().Be("Speak", "the Speak must already be in the send channel while the lock is held");
    }

    [Test]
    public void SpeakWebSocketClient_AutoFlush_Conversions_Should_Clamp_Instead_Of_Overflowing()
    {
        // The threshold is computed outside ProcessAutoFlush's try on a fire-and-forget task, so
        // an OverflowException there would end autoflush silently.
        var huge = () => SpeakWebSocketClient.MillisecondsToStopwatchTicks(decimal.MaxValue);
        huge.Should().NotThrow();
        huge().Should().Be(long.MaxValue);
        SpeakWebSocketClient.MillisecondsToStopwatchTicks(-1).Should().Be(0);
        SpeakWebSocketClient.MillisecondsToStopwatchTicks(1000).Should().Be(Stopwatch.Frequency);

        SpeakWebSocketClient.StopwatchTicksToMilliseconds(long.MaxValue).Should().BePositive("the conversion must not wrap");
        SpeakWebSocketClient.StopwatchTicksToMilliseconds(Stopwatch.Frequency).Should().BeApproximately(1000, 0.001);
    }

    [Test]
    public void SpeakClient_Flush_Should_Complete_On_A_Blocking_SynchronizationContext_While_The_Lock_Is_Held()
    {
        var options = new DeepgramWsClientOptions(apiKey: "test-api-key", onPrem: true)
        {
            AutoFlushSpeakDelta = 1,
        };
        var client = new SpeakWebSocketClient("test-api-key", options);
        var mutex = client._mutexLastDatagram;

        // Asserts Flush() never takes _mutexLastDatagram; the non-pumping context mirrors a UI caller.
        mutex.Wait(0).Should().BeTrue();

        var flushReturned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var caller = new Thread(() =>
        {
            // A UI-style thread: it blocks inside Flush() and never pumps posted continuations.
            SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());
            try
            {
                client.Flush();
                flushReturned.TrySetResult(true);
            }
            catch (Exception ex)
            {
                flushReturned.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
        };

        try
        {
            caller.Start();
            flushReturned.Task.Wait(WaitTimeout).Should().BeTrue(
                "Flush() must not wait on _mutexLastDatagram or on a continuation posted to the caller's context");
        }
        finally
        {
            mutex.Release();
        }
    }

    [Test]
    public void SpeakClient_SpeakWithText_On_A_Blocking_SynchronizationContext_Should_Not_Strand_The_Lock()
    {
        var options = new DeepgramWsClientOptions(apiKey: "test-api-key", onPrem: true)
        {
            AutoFlushSpeakDelta = 1,
        };
        var client = new SpeakWebSocketClient("test-api-key", options);
        var mutex = client._mutexLastDatagram;

        // The autoflush tick holds the lock while a UI-style caller sends a Speak.
        mutex.Wait(0).Should().BeTrue();
        var lockHeld = true;
        var startTimestamp = Stopwatch.GetTimestamp();

        var speakReturned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var caller = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());
            try
            {
                client.SpeakWithText("Hello from a UI thread.");
                speakReturned.TrySetResult(true);
            }
            catch (Exception ex)
            {
                speakReturned.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
        };

        try
        {
            caller.Start();

            // Release only once the caller is parked on the lock. CallerProbe requires it to stay
            // parked for a window, and its remarks give the residual risk.
            CallerProbe.WaitUntilReturnedOrParked(caller, speakReturned.Task, WaitTimeout)
                .Should().BeTrue("the caller should reach the lock");

            // The probe also returns true when the caller has already returned, so a regression
            // that skips InspectMessage (or makes it async again) would pass it. Require the
            // caller to still be blocked on the held lock, with _lastReceived not yet set.
            speakReturned.Task.IsCompleted.Should().BeFalse(
                "SpeakWithText must block on the held _mutexLastDatagram, not bypass InspectMessage");
            client.LastReceivedTimestamp.Should().BeNull("InspectMessage must not set _lastReceived before the lock is free");

            mutex.Release();
            lockHeld = false;

            speakReturned.Task.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue("SpeakWithText must return");
            lockHeld = mutex.Wait(TimeSpan.FromSeconds(2));
            lockHeld.Should().BeTrue(
                "InspectMessage must release _mutexLastDatagram even when the caller's context never pumps");
            client.LastReceivedTimestamp.Should().NotBeNull("InspectMessage must set _lastReceived under the lock");
            client.LastReceivedTimestamp!.Value.Should().BeInRange(startTimestamp, Stopwatch.GetTimestamp(),
                "_lastReceived must be a monotonic Stopwatch timestamp");
        }
        finally
        {
            if (lockHeld)
            {
                mutex.Release();
            }
        }
    }

    // nullByte false: SendClose -> SendMessageImmediately, which waits on the send lock in
    // AbstractWebSocketClient. nullByte true: SendClose waits on the send lock itself.
    [TestCase(false)]
    [TestCase(true)]
    public async Task SpeakClient_Close_On_A_Blocking_SynchronizationContext_Should_Complete_When_The_Send_Lock_Is_Contended(bool nullByte)
    {
        using var server = new FlushCountingWebSocketServer();

        var options = new DeepgramWsClientOptions(
            apiKey: "test-api-key",
            baseAddress: $"ws://127.0.0.1:{server.Port}/v1",
            onPrem: true);
        var client = new SpeakWebSocketClient("test-api-key", options);
        var sendLock = client.SendLock;
        var lockHeld = false;

        try
        {
            (await client.Connect(new SpeakSchema())).Should().BeTrue();

            // Hold the send lock so Close()'s WaitAsync completes asynchronously, which is when a
            // captured context matters.
            lockHeld = sendLock.Wait(WaitTimeout);
            lockHeld.Should().BeTrue();

            var closeReturned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var caller = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());
                try
                {
                    client.Close(nullByte);
                    closeReturned.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    closeReturned.TrySetException(ex);
                }
            })
            {
                IsBackground = true,
            };
            caller.Start();

            CallerProbe.WaitUntilReturnedOrParked(caller, closeReturned.Task, WaitTimeout)
                .Should().BeTrue("the caller should reach the send lock");
            closeReturned.Task.IsCompleted.Should().BeFalse("Close() must queue on the held send lock");

            sendLock.Release();
            lockHeld = false;

            closeReturned.Task.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue(
                "Close() must return once the send lock is free, even when the caller's context never pumps");
        }
        finally
        {
            if (lockHeld)
            {
                sendLock.Release();
            }

            // A regression strands _mutexSend with a continuation that never runs, and Stop() waits
            // on that lock; bound the cleanup so the failure is reported instead of hanging the run.
            await Task.WhenAny(client.Stop(), Task.Delay(WaitTimeout));
        }
    }

    /// <summary>
    /// In-process WebSocket server on a port the OS assigns and keeps bound. It performs the
    /// RFC 6455 upgrade by hand, then counts the Flush text frames it receives.
    /// </summary>
    private sealed class FlushCountingWebSocketServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly TaskCompletionSource<bool> _firstFlush = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _secondFlush = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private TcpClient? _client;

        public FlushCountingWebSocketServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(RunAsync);
        }

        public int Port { get; }

        public Task FirstFlush => _firstFlush.Task;

        public Task SecondFlush => _secondFlush.Task;

        private async Task RunAsync()
        {
            try
            {
                _client = await _listener.AcceptTcpClientAsync();
                var stream = _client.GetStream();
                await AcceptUpgradeAsync(stream);

                using var socket = WebSocket.CreateFromStream(stream, isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
                var flushCount = 0;
                while (!_cts.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    using var message = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        var buffer = new byte[1024];
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        message.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                        return;
                    }

                    if (result.MessageType != WebSocketMessageType.Text)
                    {
                        continue;
                    }

                    using var json = JsonDocument.Parse(message.ToArray());
                    if (json.RootElement.TryGetProperty("type", out var type) && type.GetString() == "Flush")
                    {
                        flushCount++;
                        if (flushCount == 1)
                        {
                            _firstFlush.TrySetResult(true);
                        }
                        else if (flushCount == 2)
                        {
                            _secondFlush.TrySetResult(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Surface a handshake or receive failure as the cause of the first gate. After the
                // first Flush this is a no-op, so test cleanup tearing the server down is harmless.
                _firstFlush.TrySetException(ex);
            }
        }

        private async Task AcceptUpgradeAsync(NetworkStream stream)
        {
            var requestBytes = new List<byte>();
            var one = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(one, 0, 1, _cts.Token);
                if (read == 0)
                {
                    throw new IOException("Client closed the connection during the upgrade.");
                }
                requestBytes.Add(one[0]);
                var len = requestBytes.Count;
                if (len >= 4 &&
                    requestBytes[len - 4] == (byte)'\r' && requestBytes[len - 3] == (byte)'\n' &&
                    requestBytes[len - 2] == (byte)'\r' && requestBytes[len - 1] == (byte)'\n')
                {
                    break;
                }
            }

            var request = Encoding.ASCII.GetString(requestBytes.ToArray());
            var keyLine = request.Split(new[] { "\r\n" }, StringSplitOptions.None)
                .First(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));
            var key = keyLine.Substring(keyLine.IndexOf(':') + 1).Trim();
            string accept;
            using (var sha1 = SHA1.Create())
            {
                accept = Convert.ToBase64String(
                    sha1.ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            }

            var response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {accept}\r\n\r\n";
            var responseBytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, _cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _client?.Dispose();
            _listener.Stop();
            _cts.Dispose();
        }
    }
}
