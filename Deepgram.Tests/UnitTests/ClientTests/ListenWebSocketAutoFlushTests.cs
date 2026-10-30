// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenWebSocketAutoFlushTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private const string InterimResults =
        """{"type":"Results","channel_index":[0,1],"duration":1.0,"start":0.0,"is_final":false,"speech_final":false,"channel":{"alternatives":[{"transcript":"hello","confidence":0.9,"words":[]}]}}""";

    private const string FinalResults =
        """{"type":"Results","channel_index":[0,1],"duration":1.0,"start":0.0,"is_final":true,"speech_final":true,"channel":{"alternatives":[{"transcript":"world","confidence":0.9,"words":[]}]}}""";

    [Test]
    public void ListenWebSocketClient_AutoFlush_Conversions_Should_Clamp_Instead_Of_Overflowing()
    {
        // The threshold is computed outside ProcessAutoFlush's try on a fire-and-forget task, so
        // an OverflowException there would end autoflush silently.
        var huge = () => ListenWebSocketClient.MillisecondsToStopwatchTicks(decimal.MaxValue);
        huge.Should().NotThrow();
        huge().Should().Be(long.MaxValue);
        ListenWebSocketClient.MillisecondsToStopwatchTicks(-1).Should().Be(0);
        ListenWebSocketClient.MillisecondsToStopwatchTicks(1000).Should().Be(Stopwatch.Frequency);

        ListenWebSocketClient.StopwatchTicksToMilliseconds(long.MaxValue).Should().BePositive("the conversion must not wrap");
        ListenWebSocketClient.StopwatchTicksToMilliseconds(Stopwatch.Frequency).Should().BeApproximately(1000, 0.001);
    }

    [Test]
    public void ListenClient_Inspecting_Results_On_A_Blocking_SynchronizationContext_Should_Not_Deadlock_Or_Strand_The_Lock()
    {
        var options = new DeepgramWsClientOptions(apiKey: "test-api-key", onPrem: true)
        {
            AutoFlushReplyDelta = 1,
        };
        var client = new ListenWebSocketClient("test-api-key", options);
        var mutex = client._mutexLastDatagram;

        // The autoflush tick holds the lock while an interim result is inspected on a thread
        // whose context never pumps, as on a UI thread.
        mutex.Wait(0).Should().BeTrue();
        var lockHeld = true;

        var payload = Encoding.UTF8.GetBytes(InterimResults);
        var processed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var caller = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());
            try
            {
                using var ms = new MemoryStream(payload);
                client.ProcessTextMessage(new WebSocketReceiveResult(payload.Length, WebSocketMessageType.Text, true), ms);
                processed.TrySetResult(true);
            }
            catch (Exception ex)
            {
                processed.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
        };

        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            caller.Start();

            // ProcessTextMessage blocks in InspectMessage(...).Wait() after WaitAsync has queued
            // its waiter. Release once the caller has stayed parked; CallerProbe's remarks give
            // the residual risk.
            CallerProbe.WaitUntilReturnedOrParked(caller, processed.Task, WaitTimeout)
                .Should().BeTrue("the caller should reach the lock");

            // The probe also returns true when the caller has already returned, so a regression
            // that skips InspectMessage would pass it. Require the caller to still be blocked on
            // the held lock, with the Interim branch not yet run.
            processed.Task.IsCompleted.Should().BeFalse(
                "ProcessTextMessage must block on the held _mutexLastDatagram, not bypass InspectMessage");
            client.LastReceivedTimestamp.Should().BeNull("the Interim branch must not run before the lock is free");

            mutex.Release();
            lockHeld = false;

            processed.Task.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue(
                "ProcessTextMessage must return once the lock is free, even when the caller's context never pumps");
            lockHeld = mutex.Wait(TimeSpan.FromSeconds(2));
            lockHeld.Should().BeTrue("InspectMessage must release _mutexLastDatagram");
            client.LastReceivedTimestamp.Should().NotBeNull("the Interim branch must set _lastReceived under the lock");
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

    [Test]
    public async Task ListenClient_AutoFlush_Should_Not_Block_Inbound_Results_While_Finalize_Is_Sending()
    {
        using var server = new FinalizeCountingWebSocketServer();

        var options = new DeepgramWsClientOptions(
            apiKey: "test-api-key",
            baseAddress: $"ws://127.0.0.1:{server.Port}/v1",
            onPrem: true)
        {
            AutoFlushReplyDelta = 50,
        };
        var client = new ListenWebSocketClient("test-api-key", options);
        var sendLock = client.SendLock;
        var datagramLock = client._mutexLastDatagram;

        var interimHandled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finalHandled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await client.Subscribe(new EventHandler<ResultResponse>((_, e) =>
        {
            if (e.IsFinal == true)
            {
                finalHandled.TrySetResult(true);
            }
            else
            {
                interimHandled.TrySetResult(true);
            }
        }));

        var lockHeld = false;
        try
        {
            (await client.Connect(new LiveSchema())).Should().BeTrue();

            // Hold the send lock before any transcript arrives, so the first autoflush Finalize
            // cannot reach the wire until the test releases it.
            lockHeld = sendLock.Wait(WaitTimeout);
            lockHeld.Should().BeTrue();

            await server.SendTextAsync(InterimResults);
            await interimHandled.Task.WaitAsync(WaitTimeout);

            // Wait until the autoflush tick has decided to finalize the interim. With the fix it has
            // cleared _lastReceived and is parked on the send lock outside _mutexLastDatagram;
            // without it, it is parked on the send lock while holding _mutexLastDatagram. Require
            // the state to hold for longer than a skip-path tick holds the lock.
            var sustained = Stopwatch.StartNew();
            var deadline = Stopwatch.StartNew();
            while (sustained.ElapsedMilliseconds < 300)
            {
                deadline.Elapsed.Should().BeLessThan(WaitTimeout, "the autoflush tick should reach SendFinalize");
                if (client.LastReceivedTimestamp != null && datagramLock.CurrentCount != 0)
                {
                    sustained.Restart();
                }
                await Task.Delay(5);
            }

            server.FinalizeCount.Should().Be(0, "the held send lock must keep the Finalize off the wire");

            // A final result takes _mutexLastDatagram in InspectMessage. It must be delivered while
            // the Finalize send is still blocked.
            await server.SendTextAsync(FinalResults);
            var delivered = await Task.WhenAny(finalHandled.Task, Task.Delay(TimeSpan.FromSeconds(2))) == finalHandled.Task;

            delivered.Should().BeTrue("inbound results must not wait for the autoflush Finalize send");
            server.FinalizeCount.Should().Be(0, "the result must have been delivered while the Finalize send was held");

            sendLock.Release();
            lockHeld = false;

            await server.FirstFinalize.WaitAsync(WaitTimeout);

            // The final result cleared _lastReceived, so later ticks must not send a second Finalize.
            await Task.Delay(TimeSpan.FromMilliseconds(1200));
            server.FinalizeCount.Should().Be(1, "autoflush must finalize the interim exactly once");
        }
        finally
        {
            if (lockHeld)
            {
                sendLock.Release();
            }

            await Task.WhenAny(client.Stop(), Task.Delay(WaitTimeout));
        }
    }

    /// <summary>
    /// In-process WebSocket server on a port the OS assigns and keeps bound. It performs the
    /// RFC 6455 upgrade by hand, sends text frames on request, and counts the Finalize frames it
    /// receives.
    /// </summary>
    private sealed class FinalizeCountingWebSocketServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly TaskCompletionSource<WebSocket> _socket = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _firstFinalize = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private TcpClient? _client;
        private int _finalizeCount;

        public FinalizeCountingWebSocketServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(RunAsync);
        }

        public int Port { get; }

        public Task FirstFinalize => _firstFinalize.Task;

        public int FinalizeCount => Volatile.Read(ref _finalizeCount);

        public async Task SendTextAsync(string text)
        {
            var socket = await _socket.Task.WaitAsync(WaitTimeout);
            var bytes = Encoding.UTF8.GetBytes(text);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        }

        private async Task RunAsync()
        {
            try
            {
                _client = await _listener.AcceptTcpClientAsync();
                var stream = _client.GetStream();
                await AcceptUpgradeAsync(stream);

                using var socket = WebSocket.CreateFromStream(stream, isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
                _socket.TrySetResult(socket);
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
                    if (json.RootElement.TryGetProperty("type", out var type) && type.GetString() == "Finalize")
                    {
                        Interlocked.Increment(ref _finalizeCount);
                        _firstFinalize.TrySetResult(true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Surface a handshake failure through the send path and the Finalize gate. After
                // they complete this is a no-op, so cleanup tearing the server down is harmless.
                _socket.TrySetException(ex);
                _firstFinalize.TrySetException(ex);
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
