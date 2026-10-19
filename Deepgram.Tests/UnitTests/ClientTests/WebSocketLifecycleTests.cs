// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.WebSocket;
using Microsoft.Extensions.Logging;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Connection lifecycle regressions from #455. Each test drives a real ClientWebSocket against a
/// loopback server and orders the steps with TaskCompletionSource gates, never with delays.
/// </summary>
[NonParallelizable] // tests install a process-wide log provider as a gate
public class WebSocketLifecycleTests
{
    // Upper bound for any single gate; reached only when a test is about to fail.
    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

    private static string NewApiKey() => new Faker().Random.Guid().ToString();

    /// <summary>
    /// Holds the first SendClose() call until the test opens <see cref="ReleaseFirstSendClose"/>.
    /// Every other call completes at once without sending anything.
    /// </summary>
    private sealed class GatedCloseClient : AbstractWebSocketClient
    {
        private int _sendCloseCalls;

        public GatedCloseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public TaskCompletionSource<bool> FirstSendCloseEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseFirstSendClose { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
        {
            if (Interlocked.Increment(ref _sendCloseCalls) == 1)
            {
                FirstSendCloseEntered.TrySetResult(true);
                await ReleaseFirstSendClose.Task;
            }
        }
    }

    /// <summary>
    /// A Flux STT client that, once its first CloseStream message is on the wire, holds the
    /// sending Stop() until the test opens <see cref="ReleaseFirstSendClose"/>.
    /// </summary>
    private sealed class GatedCloseFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        private int _closeStreamSends;

        public GatedCloseFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public TaskCompletionSource<bool> FirstSendCloseReturned { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseFirstSendClose { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            await base.SendOnConnection(owned, data, messageType, token);
            var isCloseStream = messageType == WebSocketMessageType.Text
                && Encoding.UTF8.GetString(data.Array!, data.Offset, data.Count).Contains("CloseStream");
            if (isCloseStream && Interlocked.Increment(ref _closeStreamSends) == 1)
            {
                FirstSendCloseReturned.TrySetResult(true);
                await ReleaseFirstSendClose.Task;
            }
        }
    }

    [Test]
    public async Task Stale_Stop_Should_Not_Release_A_Newer_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new GatedCloseClient(NewApiKey());

        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // This Stop() belongs to connection 1 and is held inside SendClose().
        var staleStop = client.Stop();
        await client.FirstSendCloseEntered.Task.WaitAsync(GateTimeout);

        // A second Stop() tears connection 1 down completely, and the client reconnects.
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        (await client.Connect(server.Endpoint)).Should().BeTrue();
        server.AcceptedConnections.Should().Be(2);

        // Let the stale Stop() finish. It must not release connection 2.
        client.ReleaseFirstSendClose.TrySetResult(true);
        (await staleStop.WaitAsync(GateTimeout)).Should().BeTrue();

        client.IsConnected().Should().BeTrue("a Stop() that started on connection 1 must leave connection 2 alone");

        await client.Stop();
    }

    [Test]
    public async Task FluxClient_Stop_Should_Not_Release_A_Connection_Made_During_Its_Grace_Period()
    {
        // The server closes a connection as soon as it receives CloseStream, as Flux STT does.
        using var server = new LoopbackWebSocketServer(async connection =>
        {
            while (true)
            {
                var frame = await connection.ReadFrameAsync();
                if (frame.IsText && frame.Text.Contains("CloseStream"))
                {
                    await connection.SendCloseAsync();
                    return;
                }
            }
        });
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new GatedCloseFluxClient(apiKey, options);
        var schema = new FluxSchema { Model = "flux-general-en", Encoding = "linear16", SampleRate = 16000 };

        (await client.Connect(schema)).Should().BeTrue();

        // This Stop() belongs to connection 1. It sends CloseStream, the server closes, and the
        // Stop() is held right after the CloseStream message is sent.
        var staleStop = client.Stop();
        await client.FirstSendCloseReturned.Task.WaitAsync(GateTimeout);

        // Connection 1 is fully torn down once another Stop() returns; then the client reconnects.
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        (await client.Connect(schema)).Should().BeTrue();
        server.AcceptedConnections.Should().Be(2);

        // Let the stale Stop() continue to its fallback. It must not stop connection 2.
        client.ReleaseFirstSendClose.TrySetResult(true);
        (await staleStop.WaitAsync(GateTimeout)).Should().BeTrue();

        client.IsConnected().Should().BeTrue("a Flux STT Stop() that started on connection 1 must leave connection 2 alone");

        await client.Stop();
    }

    /// <summary>
    /// Holds the sender inside its send of a marker message until the test opens
    /// <see cref="ReleaseSender"/>, ignoring cancellation, so the sender task outlives its connection.
    /// </summary>
    private sealed class BlockingSenderClient : AbstractWebSocketClient
    {
        public static readonly byte[] Marker = { 0xC1, 0xC1 };

        public BlockingSenderClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public TaskCompletionSource<bool> SenderBlocked { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseSender { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            if (data.Count == Marker.Length && data.SequenceEqual(Marker))
            {
                SenderBlocked.TrySetResult(true);
                await ReleaseSender.Task;
                return;
            }
            await base.SendOnConnection(owned, data, messageType, token);
        }
    }

    /// <summary>Completes a task when a log entry with the given text is written.</summary>
    private sealed class SignalingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _signals = new();
        private readonly ConcurrentDictionary<string, Task> _holds = new();

        public Task WhenLogged(string text) =>
            _signals.GetOrAdd(text, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)).Task;

        /// <summary>
        /// Holds the first thread that writes a log entry with the given text until
        /// <paramref name="release"/> completes (bounded by the gate timeout). Later entries with
        /// the same text are not held. <see cref="WhenLogged"/> on the same text completes once
        /// that thread is held.
        /// </summary>
        public void HoldWhenLogged(string text, Task release) => _holds[text] = release;

        public ILogger CreateLogger(string categoryName) => new SignalingLogger(this);

        public void Dispose() { }

        private sealed class SignalingLogger(SignalingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                foreach (var signal in provider._signals)
                {
                    if (message.Contains(signal.Key))
                    {
                        signal.Value.TrySetResult(true);
                    }
                }
                foreach (var hold in provider._holds)
                {
                    if (message.Contains(hold.Key) && provider._holds.TryRemove(hold.Key, out var release))
                    {
                        release.Wait(GateTimeout);
                    }
                }
            }
        }
    }

    [Test]
    public async Task Connect_Stopped_While_Waiting_For_The_Previous_Sender_Should_Return_False()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BlockingSenderClient(NewApiKey());
        var logs = new SignalingLoggerProvider();
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            var waitingForSender = logs.WhenLogged("Waiting for the previous sender to finish");

            // Connection 1's sender blocks inside a send and outlives its connection.
            (await client.Connect(server.Endpoint)).Should().BeTrue();
            client.SendBinary(BlockingSenderClient.Marker);
            await client.SenderBlocked.Task.WaitAsync(GateTimeout);
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

            // Connection 2 opens and then waits for connection 1's sender. Stop it while it waits.
            var secondConnect = client.Connect(server.Endpoint);
            await waitingForSender.WaitAsync(GateTimeout);
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

            client.ReleaseSender.TrySetResult(true);
            var connected = await secondConnect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse("a connection stopped during the sender handoff must not report success or start a sender");
                client.IsConnected().Should().BeFalse();
                server.AcceptedConnections.Should().Be(2);
            }
        }
        finally
        {
            client.ReleaseSender.TrySetResult(true);
            Log.Reset();
        }
    }

    [Test]
    public async Task Stale_Connect_Should_Not_Unpublish_A_Newer_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BlockingSenderClient(NewApiKey());
        var logs = new SignalingLoggerProvider();
        var releaseFirstConnect = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Hold connection 1's Connect() right after it has claimed the client. It runs off the
            // test thread, because the claim happens before Connect()'s first await.
            const string claimed = "Connection claimed";
            var held = logs.WhenLogged(claimed);
            logs.HoldWhenLogged(claimed, releaseFirstConnect.Task);
            var firstConnect = Task.Run(() => client.Connect(server.Endpoint));
            await held.WaitAsync(GateTimeout);

            // Stop connection 1, then open connection 2.
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
            (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

            // Let the stale connection 1 Connect() finish.
            releaseFirstConnect.TrySetResult(true);
            var firstConnected = await firstConnect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                firstConnected.Should().BeFalse("connection 1 was stopped while connecting");
                client.IsConnected().Should().BeTrue("connection 2 must stay published");
                client.State().Should().Be(WebSocketState.Open);
            }
        }
        finally
        {
            releaseFirstConnect.TrySetResult(true);
            Log.Reset();
        }

        await client.Stop();
    }

    /// <summary>
    /// A Listen client that exposes its send mutex and signals once its CloseStream send is
    /// waiting for that mutex.
    /// </summary>
    private sealed class MutexGatedListenClient : Deepgram.Clients.Listen.v2.WebSocket.Client
    {
        public MutexGatedListenClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public SemaphoreSlim SendMutex => _mutexSend;

        public TaskCompletionSource<bool> CloseStreamWaitingForMutex { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            // The base send runs synchronously up to its wait for the held mutex, so once it has
            // returned its task, it is waiting there.
            var send = base.SendOnConnection(owned, data, messageType, token);
            if (messageType == WebSocketMessageType.Text
                && Encoding.UTF8.GetString(data.Array!, data.Offset, data.Count).Contains("CloseStream"))
            {
                CloseStreamWaitingForMutex.TrySetResult(true);
            }
            await send;
        }
    }

    [Test]
    public async Task Stop_Should_Honor_The_Caller_Token_When_The_Close_Message_Is_Blocked()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new MutexGatedListenClient(apiKey, options);
        (await client.Connect(new ListenModels.LiveSchema { Model = "nova-3" })).Should().BeTrue();

        // Hold the send mutex, as a sender stalled on a TCP send would.
        (await client.SendMutex.WaitAsync(GateTimeout)).Should().BeTrue();
        try
        {
            using var timeout = new CancellationTokenSource();
            var stop = client.Stop(timeout);
            await client.CloseStreamWaitingForMutex.Task.WaitAsync(GateTimeout);

            // The caller's timeout expires while the close message waits.
            timeout.Cancel();

            (await stop.WaitAsync(GateTimeout)).Should().BeTrue("Stop() ends when the caller's token cancels");
            client.IsConnected().Should().BeFalse();
        }
        finally
        {
            client.SendMutex.Release();
        }
    }
}
