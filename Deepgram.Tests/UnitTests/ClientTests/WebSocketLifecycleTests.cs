// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.WebSocket;
using FluxSpeak = Deepgram.Models.Flux.Speak.WebSocket;
using Microsoft.Extensions.Logging;
using AgentModels = Deepgram.Models.Agent.v2.WebSocket;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using SpeakModels = Deepgram.Models.Speak.v2.WebSocket;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
using Common = Deepgram.Models.Common.v2.WebSocket;

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
    /// A client whose SendClose() sends nothing and which exposes the socket it currently holds.
    /// </summary>
    private sealed class ExposedClient : AbstractWebSocketClient
    {
        public ExposedClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public ClientWebSocket? CurrentSocket => _clientWebSocket;

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;
    }

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

    private static bool IsSettingsMessage(ArraySegment<byte> data, WebSocketMessageType messageType) =>
        messageType == WebSocketMessageType.Text
        && Encoding.UTF8.GetString(data.Array!, data.Offset, data.Count).Contains("\"Settings\"");

    /// <summary>
    /// A Voice Agent client whose first Settings message, sent by Connect(), fails to send.
    /// </summary>
    private sealed class FailingSettingsAgentClient : Deepgram.Clients.Agent.v2.WebSocket.Client
    {
        private int _failuresLeft = 1;

        public FailingSettingsAgentClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal override Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            if (IsSettingsMessage(data, messageType) && Interlocked.Exchange(ref _failuresLeft, 0) == 1)
            {
                throw new WebSocketException("Simulated Settings send failure");
            }
            return base.SendOnConnection(owned, data, messageType, token);
        }
    }

    /// <summary>
    /// A Voice Agent client that records the order of its Settings and binary sends, and lets a
    /// test hold the Settings message and see when a binary send starts.
    /// </summary>
    private sealed class OrderingAgentClient : Deepgram.Clients.Agent.v2.WebSocket.Client
    {
        public OrderingAgentClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public ConcurrentQueue<string> Events { get; } = new();

        public TaskCompletionSource<bool> SettingsEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Completed by default; a test replaces it to hold the Settings message.</summary>
        public Task ReleaseSettings { get; set; } = Task.CompletedTask;

        public TaskCompletionSource<bool> BinarySendStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            if (IsSettingsMessage(data, messageType))
            {
                SettingsEntered.TrySetResult(true);
                await ReleaseSettings.WaitAsync(GateTimeout);
                await base.SendOnConnection(owned, data, messageType, token);
                Events.Enqueue("settings");
                return;
            }
            if (messageType == WebSocketMessageType.Binary)
            {
                Events.Enqueue("binary");
                BinarySendStarted.TrySetResult(true);
            }
            await base.SendOnConnection(owned, data, messageType, token);
        }
    }

    /// <summary>
    /// A Voice Agent client whose connection drops while Connect() sends the Settings message: the
    /// send aborts the socket and returns without sending.
    /// </summary>
    private sealed class DroppedDuringSettingsAgentClient : Deepgram.Clients.Agent.v2.WebSocket.Client
    {
        private int _drops = 1;

        public DroppedDuringSettingsAgentClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal override Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            if (IsSettingsMessage(data, messageType) && Interlocked.Exchange(ref _drops, 0) == 1)
            {
                owned.Socket.Abort();
                return Task.CompletedTask;
            }
            return base.SendOnConnection(owned, data, messageType, token);
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
    public async Task Connect_Should_Honor_The_Caller_Token_While_Waiting_For_The_Previous_Sender()
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

            (await client.Connect(server.Endpoint)).Should().BeTrue();
            client.SendBinary(BlockingSenderClient.Marker);
            await client.SenderBlocked.Task.WaitAsync(GateTimeout);
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

            using var timeout = new CancellationTokenSource();
            var secondConnect = client.Connect(server.Endpoint, timeout);
            await waitingForSender.WaitAsync(GateTimeout);

            timeout.Cancel();
            (await secondConnect.WaitAsync(TimeSpan.FromSeconds(1))).Should().BeFalse(
                "the caller's Connect cancellation token bounds the previous-sender wait");
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

    [Test]
    public async Task Stop_Should_Dispose_The_Socket()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());

        (await client.Connect(server.Endpoint)).Should().BeTrue();
        var socket = client.CurrentSocket;
        socket.Should().NotBeNull();

        (await client.Stop()).Should().BeTrue();

        Func<Task> send = () => socket!.SendAsync(new ArraySegment<byte>(new byte[] { 1 }), WebSocketMessageType.Binary, true, CancellationToken.None);
        await send.Should().ThrowAsync<ObjectDisposedException>("Stop() must dispose the socket it releases");
    }

    [Test]
    public async Task Connect_From_A_Close_Handler_Should_Open_A_New_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var reconnect = new TaskCompletionSource<Task<bool>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) =>
        {
            if (Interlocked.Increment(ref closeEvents) == 1)
            {
                // Reconnect from inside the handler, as an app that reconnects on close would.
                reconnect.TrySetResult(client.Connect(server.Endpoint));
            }
        }));

        (await client.Connect(server.Endpoint)).Should().BeTrue();
        (await client.Stop()).Should().BeTrue();

        var reconnectTask = await reconnect.Task.WaitAsync(GateTimeout);
        var reconnected = await reconnectTask.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            reconnected.Should().BeTrue();
            client.IsConnected().Should().BeTrue("a Connect() from a Close handler must open a real connection");
            server.AcceptedConnections.Should().Be(2);
        }

        await client.Stop();
    }

    /// <summary>A client whose SendClose() runs the test's script.</summary>
    private sealed class ScriptedCloseClient : AbstractWebSocketClient
    {
        private readonly Func<Task> _sendClose;

        public ScriptedCloseClient(string apiKey, Func<Task> sendClose)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false })
        {
            _sendClose = sendClose;
        }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) => _sendClose();
    }

    [Test]
    public async Task Concurrent_Stops_Should_Raise_Close_Once()
    {
        using var server = new LoopbackWebSocketServer();
        var entered = 0;
        var bothEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSendClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new ScriptedCloseClient(NewApiKey(), async () =>
        {
            if (Interlocked.Increment(ref entered) == 2)
            {
                bothEntered.TrySetResult(true);
            }
            await releaseSendClose.Task;
        });
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // Both Stop() calls get past every "already released" check before either one goes on.
        var firstStop = client.Stop();
        var secondStop = client.Stop();
        await bothEntered.Task.WaitAsync(GateTimeout);
        releaseSendClose.TrySetResult(true);
        var results = await Task.WhenAll(firstStop, secondStop).WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            results.Should().AllBeEquivalentTo(true);
            Volatile.Read(ref closeEvents).Should().Be(1, "one connection was stopped, so Close fires once");
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task Stop_Should_Raise_Close_When_The_Close_Handshake_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedCloseClient(NewApiKey(),
            () => throw new WebSocketException("Simulated close handshake failure"));
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        (await client.Connect(server.Endpoint)).Should().BeTrue();
        var stopped = await client.Stop().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            stopped.Should().BeTrue();
            Volatile.Read(ref closeEvents).Should().Be(1, "a Stop() whose close handshake fails still ends the connection");
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task Stop_Should_Raise_Close_When_The_Close_Handshake_Times_Out()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedCloseClient(NewApiKey(), () => Task.CompletedTask);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // A disconnect timeout that has already expired.
        using var expired = new CancellationTokenSource();
        expired.Cancel();
        var stopped = await client.Stop(expired).WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            stopped.Should().BeTrue();
            Volatile.Read(ref closeEvents).Should().Be(1, "a Stop() that times out still ends the connection");
            client.IsConnected().Should().BeFalse();
        }
    }

    /// <summary>A loopback server that closes a connection once it receives the given text message.</summary>
    private static LoopbackWebSocketServer ServerClosingOn(string messageType) => new(async connection =>
    {
        while (true)
        {
            var frame = await connection.ReadFrameAsync();
            if (frame.IsText && frame.Text.Contains($"\"{messageType}\""))
            {
                await connection.SendCloseAsync();
                return;
            }
        }
    });

    [Test]
    public async Task FluxClient_Stop_Should_Deliver_Close_To_Every_Subscriber_Once()
    {
        using var server = ServerClosingOn("CloseStream");
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.WebSocket.Client(apiKey, options);
        var first = 0;
        var second = 0;
        await client.Subscribe(new EventHandler<CloseResponse>((_, _) => Interlocked.Increment(ref first)));
        await client.Subscribe(new EventHandler<CloseResponse>((_, _) => Interlocked.Increment(ref second)));

        (await client.Connect(new FluxSchema { Model = "flux-general-en" })).Should().BeTrue();
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        using (new AssertionScope())
        {
            Volatile.Read(ref first).Should().Be(1);
            Volatile.Read(ref second).Should().Be(1, "every Close subscriber receives the event");
        }
    }

    [Test]
    public async Task FluxSpeakClient_Stop_Should_Deliver_Close_To_Every_Subscriber_Once()
    {
        using var server = ServerClosingOn("Close");
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, options);
        var first = 0;
        var second = 0;
        await client.Subscribe(new EventHandler<FluxSpeak.CloseResponse>((_, _) => Interlocked.Increment(ref first)));
        await client.Subscribe(new EventHandler<FluxSpeak.CloseResponse>((_, _) => Interlocked.Increment(ref second)));

        (await client.Connect(new FluxSpeak.SpeakSchema { Model = "flux-alexis-en" })).Should().BeTrue();
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        using (new AssertionScope())
        {
            Volatile.Read(ref first).Should().Be(1);
            Volatile.Read(ref second).Should().Be(1, "every Close subscriber receives the event");
        }
    }

    [Test]
    public async Task Stop_During_The_Handshake_Should_Not_Raise_Close()
    {
        // Hold the connection inside its handshake: Open never fires for it.
        var upgradeRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeUpgrade = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(beforeUpgrade: async () =>
        {
            upgradeRequested.TrySetResult(true);
            await completeUpgrade.Task;
        });
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var connect = client.Connect(server.Endpoint);
        await upgradeRequested.Task.WaitAsync(GateTimeout);
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        completeUpgrade.TrySetResult(true);
        var connected = await connect.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            connected.Should().BeFalse("Stop() ended the connection during its handshake");
            Volatile.Read(ref openEvents).Should().Be(0);
            Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open, and Open never fired");
        }
    }

    [Test]
    public async Task Connect_Stopped_Before_Open_Should_Not_Raise_Open_Or_Report_Success()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var logs = new SignalingLoggerProvider();
        var releaseConnect = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Hold Connect() after it has checked for a release and started its sender, just
            // before it marks the connection opened.
            const string senderStarting = "Sender Thread started";
            var held = logs.WhenLogged(senderStarting);
            logs.HoldWhenLogged(senderStarting, releaseConnect.Task);

            var connect = client.Connect(server.Endpoint);
            await held.WaitAsync(GateTimeout);

            // A Stop() whose close handshake has already timed out releases the connection now.
            using var expired = new CancellationTokenSource();
            expired.Cancel();
            (await client.Stop(expired).WaitAsync(GateTimeout)).Should().BeTrue();

            releaseConnect.TrySetResult(true);
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse("the connection was released before Open");
                client.IsConnected().Should().BeFalse();
                Volatile.Read(ref openEvents).Should().Be(0, "Open must not fire for a released connection");
                Volatile.Read(ref closeEvents).Should().Be(0);
            }
        }
        finally
        {
            releaseConnect.TrySetResult(true);
            Log.Reset();
        }
    }

    /// <summary>
    /// Holds the first SendClose() call until the test completes <see cref="Release"/>. The gate
    /// runs its continuation inline, so the rest of that Stop() runs inside Release.TrySetResult().
    /// </summary>
    private sealed class InlineGatedCloseClient : AbstractWebSocketClient
    {
        private int _calls;

        public InlineGatedCloseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Release { get; } = new();

        public override async Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
        {
            if (Interlocked.Increment(ref _calls) == 1)
            {
                Entered.TrySetResult(true);
                await Release.Task;
            }
        }
    }

    [Test]
    public async Task Stop_Should_Return_After_Close_Raised_By_Another_Teardown()
    {
        // The server closes the connection when the test says so.
        var serverClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(async connection =>
        {
            await serverClose.Task;
            await connection.SendCloseAsync();
        });
        using var client = new InlineGatedCloseClient(NewApiKey());
        var closeEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) =>
        {
            closeEntered.TrySetResult(true);
            finishClose.Task.Wait(GateTimeout);
        }));

        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // The user Stop() runs off the test's context and is held inside SendClose().
        var userStop = Task.Run(() => client.Stop());
        await client.Entered.Task.WaitAsync(GateTimeout);

        // The server closes; the receive loop wins the release and is now delivering Close.
        serverClose.TrySetResult(true);
        await closeEntered.Task.WaitAsync(GateTimeout);

        try
        {
            // Let the user Stop() go. It runs inline here, finds the connection released by the
            // receive loop, and must keep waiting while Close is still being delivered.
            client.Release.TrySetResult(true);
            userStop.IsCompleted.Should().BeFalse("Stop() returns only after Close has been delivered");
        }
        finally
        {
            finishClose.TrySetResult(true);
        }

        (await userStop.WaitAsync(GateTimeout)).Should().BeTrue();
    }

    [Test]
    public async Task Server_Close_At_Once_Should_Raise_Open_Then_Close()
    {
        // The server closes every connection right after the upgrade. The receive loop starts
        // only after the Open handlers return, so Close must follow a completed Open.
        using var server = new LoopbackWebSocketServer(connection => connection.SendCloseAsync());
        using var client = new ExposedClient(NewApiKey());
        var closeRaised = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var openEvents = 0;
        var closeEvents = 0;
        var openEventsSeenByClose = -1;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) =>
        {
            Interlocked.CompareExchange(ref openEventsSeenByClose, Volatile.Read(ref openEvents), -1);
            Interlocked.Increment(ref closeEvents);
            closeRaised.TrySetResult(true);
        }));

        await client.Connect(server.Endpoint);
        await closeRaised.Task.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            Volatile.Read(ref openEventsSeenByClose).Should().Be(1, "Open is delivered before Close");
            Volatile.Read(ref openEvents).Should().Be(1);
            Volatile.Read(ref closeEvents).Should().Be(1);
        }
    }

    [Test]
    public async Task Connect_Should_Return_False_When_An_Open_Handler_Releases_The_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => client.Dispose()));

        var connected = await client.Connect(server.Endpoint);

        using (new AssertionScope())
        {
            connected.Should().BeFalse("the connection was released before Connect() returned");
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task Stop_During_Open_Handlers_Should_Raise_Close_After_Open()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var events = new ConcurrentQueue<string>();
        var openEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseOpen = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) =>
        {
            events.Enqueue("open-start");
            openEntered.TrySetResult(true);
            releaseOpen.Task.Wait(GateTimeout);
            events.Enqueue("open-end");
        }));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => events.Enqueue("close")));

        var connect = Task.Run(() => client.Connect(server.Endpoint));
        await openEntered.Task.WaitAsync(GateTimeout);

        try
        {
            // Stop() ends the connection while its Open handler is still running. It does not
            // wait for the Open handler, and Close is not raised yet.
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
            events.Should().NotContain("close", "Close must not fire while Open is still being delivered");
        }
        finally
        {
            releaseOpen.TrySetResult(true);
        }

        var connected = await connect.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            connected.Should().BeFalse("the connection was stopped before Connect() returned");
            events.Should().Equal(new[] { "open-start", "open-end", "close" }, "Close follows Open, once");
        }
    }

    [Test]
    public async Task Close_Should_Wait_For_Every_Open_Handler_When_Stop_Races_Them()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var events = new ConcurrentQueue<string>();
        var firstEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) =>
        {
            firstEntered.TrySetResult(true);
            releaseFirst.Task.Wait(GateTimeout);
            events.Enqueue("open-1");
        }));
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => events.Enqueue("open-2")));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => events.Enqueue("close")));

        var connect = Task.Run(() => client.Connect(server.Endpoint));
        await firstEntered.Task.WaitAsync(GateTimeout);

        try
        {
            // A concurrent Stop() while the slow first Open handler runs.
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        }
        finally
        {
            releaseFirst.TrySetResult(true);
        }

        await connect.WaitAsync(GateTimeout);

        events.Should().Equal(new[] { "open-1", "open-2", "close" }, "Close starts only after every Open handler has returned");
    }

    [Test]
    public async Task Stop_From_An_Open_Handler_Should_Complete_And_Raise_Close_After_Open()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var events = new ConcurrentQueue<string>();
        var stopCompletedInHandler = false;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) =>
        {
            // A blocking Stop() from inside the Open handler must not deadlock.
            var stop = client.Stop();
            stopCompletedInHandler = stop.Wait(GateTimeout) && stop.Result;
            events.Enqueue("open-end");
        }));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => events.Enqueue("close")));

        var connected = await client.Connect(server.Endpoint).WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            stopCompletedInHandler.Should().BeTrue("Stop() from an Open handler completes without waiting for Open delivery");
            connected.Should().BeFalse();
            events.Should().Equal(new[] { "open-end", "close" }, "Close follows Open, once");
        }
    }

    [Test]
    public async Task Connect_Should_Succeed_When_An_Open_Handler_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var laterHandlerRan = false;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => throw new InvalidOperationException("Open handler failure")));
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => laterHandlerRan = true));

        var connected = await client.Connect(server.Endpoint);

        using (new AssertionScope())
        {
            connected.Should().BeTrue("an Open handler exception is logged, not thrown from Connect()");
            client.IsConnected().Should().BeTrue();
            server.AcceptedConnections.Should().Be(1);
            laterHandlerRan.Should().BeTrue("the next Open handler still runs");
        }

        await client.Stop();
    }

    [Test]
    public async Task AgentClient_Connect_After_A_Failed_Settings_Send_Should_Open_A_New_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new FailingSettingsAgentClient(apiKey, options);
        var settings = new AgentModels.SettingsSchema();
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        Func<Task<bool>> connect = async () => await client.Connect(settings);

        await connect.Should().ThrowAsync<WebSocketException>();
        using (new AssertionScope())
        {
            client.IsConnected().Should().BeFalse("a connection whose Settings message failed must be released");
            Volatile.Read(ref openEvents).Should().Be(0, "Settings is sent before Open, so a failed Settings raises no Open");
            Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open");
        }

        var connected = await client.Connect(settings);
        var secondConnection = await server.WaitForConnectionAsync(2).WaitAsync(GateTimeout);
        var settingsFrame = await secondConnection.ReadFrameAsync().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            connected.Should().BeTrue();
            client.IsConnected().Should().BeTrue("the retry must open a new connection");
            server.AcceptedConnections.Should().Be(2);
            settingsFrame.Text.Should().Contain("\"Settings\"", "the retry must send the Settings message on the new connection");
        }

        // Positive control: the retried connection raised Open, so its Stop() raises Close once.
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        using (new AssertionScope())
        {
            Volatile.Read(ref openEvents).Should().Be(1);
            Volatile.Read(ref closeEvents).Should().Be(1);
        }
    }

    [Test]
    public async Task AgentClient_Server_Close_Right_After_The_Upgrade_Should_Raise_Open_Then_Close()
    {
        // The server closes every connection right after the upgrade. Settings goes out before the
        // receive loop starts, and a socket reports the server's close only once it reads it, so
        // the Settings send succeeds: Connect() returns true, Open is raised, and the receive loop
        // then raises Close.
        using var server = new LoopbackWebSocketServer(connection => connection.SendCloseAsync());
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Agent.v2.WebSocket.Client(apiKey, options);
        var openEvents = 0;
        var closeEvents = 0;
        var closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) =>
        {
            Interlocked.Increment(ref closeEvents);
            closed.TrySetResult(true);
        }));

        var connected = await client.Connect(new AgentModels.SettingsSchema()).WaitAsync(GateTimeout);
        await closed.Task.WaitAsync(GateTimeout);
        var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
        var firstFrame = await connection.ReadFrameAsync().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            connected.Should().BeTrue("the Settings message was sent before the server's close was read");
            firstFrame.Text.Should().Contain("\"Settings\"");
            Volatile.Read(ref openEvents).Should().Be(1);
            Volatile.Read(ref closeEvents).Should().Be(1, "the receive loop reads the server's close after Open");
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task AgentClient_Connect_Should_Fail_When_The_Connection_Drops_During_Settings()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new DroppedDuringSettingsAgentClient(apiKey, options);
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var connected = await client.Connect(new AgentModels.SettingsSchema()).WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            Volatile.Read(ref openEvents).Should().Be(0, "Settings is sent before Open, so a connection that drops during it raises no Open");
            Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open");
            connected.Should().BeFalse("the Settings message never reached the agent");
            client.IsConnected().Should().BeFalse();
            client.State().Should().Be(WebSocketState.None, "the dropped connection must be released so a retry can connect");
        }
    }

    [Test]
    public async Task AgentClient_Should_Send_Settings_Before_Raising_Open()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => client.Events.Enqueue("open")));

        (await client.Connect(new AgentModels.SettingsSchema()).WaitAsync(GateTimeout)).Should().BeTrue();

        client.Events.Should().Equal(new[] { "settings", "open" }, "the agent is configured before Open is raised");

        await client.Stop();
    }

    [Test]
    public async Task AgentClient_Should_Send_Settings_Before_Queued_Audio()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);

        // The Open handler waits until the queued audio is being sent, so the order on the wire is
        // decided before Connect() returns.
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) =>
            client.BinarySendStarted.Task.Wait(GateTimeout)));

        // Audio queued before Connect().
        client.SendBinary(new byte[] { 1, 2, 3 });

        (await client.Connect(new AgentModels.SettingsSchema()).WaitAsync(GateTimeout)).Should().BeTrue();
        var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
        var first = await connection.ReadFrameAsync().WaitAsync(GateTimeout);
        var second = await connection.ReadFrameAsync().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            first.Text.Should().Contain("\"Settings\"", "Settings is the first frame on the wire");
            second.IsText.Should().BeFalse("the queued audio follows Settings");
            client.Events.Should().Equal(new[] { "settings", "binary" });
        }

        await client.Stop();
    }

    [Test]
    public async Task AgentClient_Immediate_Send_Before_Open_Should_Not_Precede_Settings()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        var releaseSettings = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ReleaseSettings = releaseSettings.Task;

        var connect = client.Connect(new AgentModels.SettingsSchema());
        await client.SettingsEntered.Task.WaitAsync(GateTimeout);

        // An immediate send while the Settings message is held: the connection is not open yet.
        await client.SendMessageImmediately(Encoding.UTF8.GetBytes("{\"type\":\"InjectUserMessage\",\"content\":\"early\"}"))
            .WaitAsync(GateTimeout);

        releaseSettings.TrySetResult(true);
        (await connect.WaitAsync(GateTimeout)).Should().BeTrue();

        // A sentinel: everything sent before it is on the wire by the time it arrives.
        await client.SendKeepAlive().WaitAsync(GateTimeout);
        var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
        var before = new List<string>();
        while (true)
        {
            var frame = await connection.ReadFrameAsync().WaitAsync(GateTimeout);
            if (frame.Text.Contains("\"KeepAlive\""))
            {
                break;
            }
            before.Add(frame.Text);
        }

        using (new AssertionScope())
        {
            before.Should().HaveCount(1, "the immediate send before Open is dropped");
            before[0].Should().Contain("\"Settings\"");
        }

        await client.Stop();
    }

    [Test]
    public async Task AgentClient_Audio_Sent_From_An_Open_Handler_Should_Follow_Settings()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) =>
        {
            // Start streaming as soon as the connection opens, and wait until that audio is being sent.
            client.SendBinary(new byte[] { 4, 5, 6 });
            client.BinarySendStarted.Task.Wait(GateTimeout);
        }));

        (await client.Connect(new AgentModels.SettingsSchema()).WaitAsync(GateTimeout)).Should().BeTrue();
        var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
        var first = await connection.ReadFrameAsync().WaitAsync(GateTimeout);
        var second = await connection.ReadFrameAsync().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            first.Text.Should().Contain("\"Settings\"");
            second.IsText.Should().BeFalse();
            client.Events.Should().Equal(new[] { "settings", "binary" });
        }

        await client.Stop();
    }

    [Test]
    public async Task Stop_Before_Open_Should_Fail_The_Connect_Without_Open_Or_Close()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        var releaseSettings = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ReleaseSettings = releaseSettings.Task;
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var logs = new SignalingLoggerProvider();
        var releaseStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Hold the Stop() right after it has begun (marked the connection stopping) and before
            // it does anything else.
            const string stopBegun = "Using default disconnect cancellation token";
            var stopHeld = logs.WhenLogged(stopBegun);
            logs.HoldWhenLogged(stopBegun, releaseStop.Task);

            // Connect() is held in its pre-open step, with the Settings message not yet sent. Its
            // budget outlasts every gate, so a stall fails the test instead of passing it.
            using var connectCts = new CancellationTokenSource(GateTimeout * 3);
            var connect = client.Connect(new AgentModels.SettingsSchema(), connectCts);
            await client.SettingsEntered.Task.WaitAsync(GateTimeout);

            var stop = Task.Run(() => client.Stop());
            await stopHeld.WaitAsync(GateTimeout);

            // Settings goes out on a still-open socket, but a Stop() has begun: the connect fails.
            releaseSettings.TrySetResult(true);
            var connected = await connect.WaitAsync(GateTimeout);

            releaseStop.TrySetResult(true);
            var stopped = await stop.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse("a Stop() began before the connection opened");
                stopped.Should().BeTrue();
                Volatile.Read(ref openEvents).Should().Be(0, "Open must not fire for a connection being stopped");
                Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open");
                client.IsConnected().Should().BeFalse();
            }
        }
        finally
        {
            releaseSettings.TrySetResult(true);
            releaseStop.TrySetResult(true);
            Log.Reset();
        }
    }

    [Test]
    public async Task Stop_Should_Raise_Close_When_Logging_The_Close_Event_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        using var connectCts = new CancellationTokenSource(GateTimeout);
        (await client.Connect(server.Endpoint, connectCts).WaitAsync(GateTimeout)).Should().BeTrue();

        // The provider throws on the log line that announces the Close event.
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(new ThrowingLoggerProvider("Sending CloseResponse event"));
        }));
        try
        {
            Func<Task<bool>> stop = () => client.Stop().WaitAsync(GateTimeout);
            (await stop.Should().NotThrowAsync()).Subject.Should().BeTrue();
        }
        finally
        {
            Log.Reset();
        }

        using (new AssertionScope())
        {
            Volatile.Read(ref openEvents).Should().Be(1);
            Volatile.Read(ref closeEvents).Should().Be(1, "a failing log provider must not suppress Close");
        }
    }

    [Test]
    public async Task Stop_Between_Sender_Start_And_Open_Should_Raise_Neither_Open_Nor_Close()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var logs = new SignalingLoggerProvider();
        var releaseConnect = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Hold Connect() after its sender started, just before it marks the connection opened.
            const string beforeMark = "Sending OpenResponse event";
            var connectHeld = logs.WhenLogged(beforeMark);
            logs.HoldWhenLogged(beforeMark, releaseConnect.Task);
            using var connectCts = new CancellationTokenSource(GateTimeout * 3);
            var connect = Task.Run(() => client.Connect(server.Endpoint, connectCts));
            await connectHeld.WaitAsync(GateTimeout);

            // A Stop() marks the connection stopping and is held right after, before any release.
            const string stopBegun = "Using default disconnect cancellation token";
            var stopHeld = logs.WhenLogged(stopBegun);
            logs.HoldWhenLogged(stopBegun, releaseStop.Task);
            var stop = Task.Run(() => client.Stop());
            await stopHeld.WaitAsync(GateTimeout);

            // The mark now fails on the stopping connection, so Open is never raised.
            releaseConnect.TrySetResult(true);
            var connected = await connect.WaitAsync(GateTimeout);
            releaseStop.TrySetResult(true);
            var stopped = await stop.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse();
                stopped.Should().BeTrue();
                Volatile.Read(ref openEvents).Should().Be(0, "a Stop() began before the connection was marked opened");
                Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open");
            }
        }
        finally
        {
            releaseConnect.TrySetResult(true);
            releaseStop.TrySetResult(true);
            Log.Reset();
        }
    }

    [Test]
    public async Task AgentClient_Concurrent_Connect_Should_Not_Release_The_First_Connection()
    {
        // Hold the first connection inside its handshake until the test lets it finish.
        var upgradeRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeUpgrade = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(beforeUpgrade: async () =>
        {
            upgradeRequested.TrySetResult(true);
            await completeUpgrade.Task;
        });
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Agent.v2.WebSocket.Client(apiKey, options);
        var settings = new AgentModels.SettingsSchema();

        var firstConnect = client.Connect(settings);
        await upgradeRequested.Task.WaitAsync(GateTimeout);

        // A second Connect() while the first is still in its handshake waits for it.
        var secondConnect = client.Connect(settings);
        secondConnect.IsCompleted.Should().BeFalse("the connection is not ready while it is still handshaking");

        completeUpgrade.TrySetResult(true);
        var firstConnected = await firstConnect.WaitAsync(GateTimeout);
        var secondConnected = await secondConnect.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            secondConnected.Should().BeTrue("the second Connect() returns true once the first connection is ready");
            firstConnected.Should().BeTrue("the second Connect() must not release the first connection");
            client.IsConnected().Should().BeTrue();
            server.AcceptedConnections.Should().Be(1);
        }

        // The Settings messages must all be on the wire before this KeepAlive.
        await client.SendKeepAlive();
        var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
        var frames = new List<string>();
        while (true)
        {
            var frame = await connection.ReadFrameAsync().WaitAsync(GateTimeout);
            if (frame.Text.Contains("\"KeepAlive\""))
            {
                break;
            }
            frames.Add(frame.Text);
        }

        using (new AssertionScope())
        {
            frames.Count(f => f.Contains("\"Settings\"")).Should().Be(1, "only the Connect() that opened the connection sends Settings");
            frames.Should().NotBeEmpty();
            frames[0].Should().Contain("\"Settings\"", "Settings is the first frame");
        }

        await client.Stop();
    }

    [Test]
    public async Task Dropped_Connection_Should_Raise_Close_And_Allow_A_New_Connect()
    {
        // The server drops the first connection's TCP connection without a close frame.
        using var server = new LoopbackWebSocketServer(connection =>
        {
            if (connection.Number == 1)
            {
                connection.Abort();
            }
            return Task.CompletedTask;
        });
        using var client = new ExposedClient(NewApiKey());
        var closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) =>
        {
            Interlocked.Increment(ref closeEvents);
            closed.TrySetResult(true);
        }));

        (await client.Connect(server.Endpoint)).Should().BeTrue();
        await closed.Task.WaitAsync(GateTimeout);

        var reconnected = await client.Connect(server.Endpoint);

        using (new AssertionScope())
        {
            Volatile.Read(ref closeEvents).Should().Be(1, "a dropped connection is torn down once");
            reconnected.Should().BeTrue();
            client.IsConnected().Should().BeTrue("Connect() after a dropped connection must open a new one");
            server.AcceptedConnections.Should().Be(2);
        }

        await client.Stop();
    }

    [Test]
    public async Task FluxClient_Stop_With_A_Cancelled_Token_Should_Release_And_Raise_Close()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.WebSocket.Client(apiKey, options);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        (await client.Connect(new FluxSchema { Model = "flux-general-en" })).Should().BeTrue();

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Func<Task<bool>> stop = () => client.Stop(cancelled, nullByte: true);

        (await stop.Should().NotThrowAsync()).Subject.Should().BeTrue();
        using (new AssertionScope())
        {
            Volatile.Read(ref closeEvents).Should().Be(1);
            client.IsConnected().Should().BeFalse();
            client.State().Should().Be(WebSocketState.None, "Stop() must release the connection");
        }
    }

    [Test]
    public async Task FluxSpeakClient_Stop_With_A_Cancelled_Token_Should_Release_And_Raise_Close()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, options);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<FluxSpeak.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        (await client.Connect(new FluxSpeak.SpeakSchema { Model = "flux-alexis-en" })).Should().BeTrue();

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Func<Task<bool>> stop = () => client.Stop(cancelled, nullByte: true);

        (await stop.Should().NotThrowAsync()).Subject.Should().BeTrue();
        using (new AssertionScope())
        {
            Volatile.Read(ref closeEvents).Should().Be(1);
            client.IsConnected().Should().BeFalse();
            client.State().Should().Be(WebSocketState.None, "Stop() must release the connection");
        }
    }

    /// <summary>
    /// A Flux STT client that exposes its send mutex and signals once its CloseStream send is
    /// waiting for that mutex.
    /// </summary>
    private sealed class MutexGatedFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        public MutexGatedFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

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
    public async Task FluxClient_Stop_Should_Not_Throw_When_The_Connection_Is_Released_During_Its_Close()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new MutexGatedFluxClient(apiKey, options);
        (await client.Connect(new FluxSchema { Model = "flux-general-en" })).Should().BeTrue();

        // Hold the send mutex so Stop()'s CloseStream send waits for it.
        (await client.SendMutex.WaitAsync(GateTimeout)).Should().BeTrue();
        try
        {
            var stop = client.Stop();
            await client.CloseStreamWaitingForMutex.Task.WaitAsync(GateTimeout);

            // Release the connection while the close is in flight; this cancels its token.
            client.Dispose();

            Func<Task<bool>> awaitStop = () => stop.WaitAsync(GateTimeout);
            (await awaitStop.Should().NotThrowAsync()).Subject.Should().BeTrue();
        }
        finally
        {
            client.SendMutex.Release();
        }
    }

    [Test]
    public async Task AgentClient_Stop_Before_Open_Should_Send_No_Close_Message()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        var releaseSettings = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ReleaseSettings = releaseSettings.Task;

        try
        {
            // Connect() is held in its pre-open step: the connection has not raised Open.
            var connect = client.Connect(new AgentModels.SettingsSchema());
            await client.SettingsEntered.Task.WaitAsync(GateTimeout);

            // Stop() runs to completion on the never-opened connection.
            (await Task.Run(() => client.Stop()).WaitAsync(GateTimeout)).Should().BeTrue();

            releaseSettings.TrySetResult(true);
            (await connect.WaitAsync(GateTimeout)).Should().BeFalse();

            // The server has read everything up to the client's close frame.
            var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
            await connection.CloseFrameReceived.WaitAsync(GateTimeout);

            server.DataFrames.Should().BeEmpty(
                "a connection that never raised Open gets no close message: nothing may reach the agent ahead of Settings");
        }
        finally
        {
            releaseSettings.TrySetResult(true);
        }
    }

    [Test]
    public async Task Connect_Stopping_Before_The_Sender_Starts_Should_Keep_Queued_Audio_For_The_Next_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var audio = new byte[] { 0xA7, 0xA7, 0xA7 };
        var logs = new SignalingLoggerProvider();
        var releaseConnect = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Audio queued before Connect().
            client.SendBinary(audio);

            // Hold Connect() right after it claimed the client, before its handshake.
            const string claimed = "Connection claimed";
            var connectHeld = logs.WhenLogged(claimed);
            logs.HoldWhenLogged(claimed, releaseConnect.Task);
            using var connectCts = new CancellationTokenSource(GateTimeout * 3);
            var connect = Task.Run(() => client.Connect(server.Endpoint, connectCts));
            await connectHeld.WaitAsync(GateTimeout);

            // A Stop() marks the connection stopping and is held right after.
            const string stopBegun = "Using default disconnect cancellation token";
            var stopHeld = logs.WhenLogged(stopBegun);
            logs.HoldWhenLogged(stopBegun, releaseStop.Task);
            var stop = Task.Run(() => client.Stop());
            await stopHeld.WaitAsync(GateTimeout);

            // The handshake completes, but the connection is stopping: no sender starts.
            releaseConnect.TrySetResult(true);
            (await connect.WaitAsync(GateTimeout)).Should().BeFalse();

            releaseStop.TrySetResult(true);
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();

            // Connection 1 received no data: its reader ends at the close frame, with nothing before it.
            var first = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
            await first.Closed.WaitAsync(GateTimeout);
            server.DataFrames.Should().BeEmpty("no sender ran for the stopped connection, so the queued audio was not sent on it");

            // The audio stayed queued and goes out on the next successful Connect().
            (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();
            var second = await server.WaitForConnectionAsync(2).WaitAsync(GateTimeout);
            var frame = await second.ReadFrameAsync().WaitAsync(GateTimeout);
            frame.Payload.Should().Equal(audio);
        }
        finally
        {
            releaseConnect.TrySetResult(true);
            releaseStop.TrySetResult(true);
            Log.Reset();
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_Should_Return_False_When_A_Stop_Begins_While_Open_Handlers_Run()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new GatedCloseClient(NewApiKey());
        var events = new ConcurrentQueue<string>();
        Task<bool>? stop = null;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) =>
        {
            events.Enqueue("open");
            // Another thread's Stop() begins and is held in SendClose(): the connection is stopping
            // but not yet released when Connect() returns.
            stop = Task.Run(() => client.Stop());
            client.FirstSendCloseEntered.Task.Wait(GateTimeout);
        }));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => events.Enqueue("close")));

        bool connected;
        try
        {
            connected = await client.Connect(server.Endpoint).WaitAsync(GateTimeout);
        }
        finally
        {
            client.ReleaseFirstSendClose.TrySetResult(true);
        }
        (await stop!.WaitAsync(GateTimeout)).Should().BeTrue();

        using (new AssertionScope())
        {
            connected.Should().BeFalse("a Stop() had begun on the connection before Connect() returned");
            events.Should().Equal(new[] { "open", "close" }, "Open was raised, so the Stop() raises Close once, after it");
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task AgentClient_Settings_Send_After_Stop_Closed_The_Socket_Should_Fail_The_Connect_Without_Throwing()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new OrderingAgentClient(apiKey, options);
        var releaseSettings = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ReleaseSettings = releaseSettings.Task;
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<AgentModels.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        var logs = new SignalingLoggerProvider();
        var releaseStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            // Hold the Stop() after it has sent its close frame (the socket is CloseSent) and before
            // it releases the connection.
            const string releasing = "Releasing the connection...";
            var stopHeld = logs.WhenLogged(releasing);
            logs.HoldWhenLogged(releasing, releaseStop.Task);

            // Connect() is held in its pre-open step, past its "stopping" check, before the send.
            using var connectCts = new CancellationTokenSource(GateTimeout * 3);
            var connect = client.Connect(new AgentModels.SettingsSchema(), connectCts);
            await client.SettingsEntered.Task.WaitAsync(GateTimeout);

            var stop = Task.Run(() => client.Stop());
            await stopHeld.WaitAsync(GateTimeout);

            // The Settings send now fails on the closing socket. That is the Stop()'s doing, so
            // Connect() reports false instead of throwing.
            releaseSettings.TrySetResult(true);
            Func<Task<bool>> awaitConnect = () => connect.WaitAsync(GateTimeout);
            var connected = (await awaitConnect.Should().NotThrowAsync()).Subject;

            releaseStop.TrySetResult(true);
            var stopped = await stop.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse();
                stopped.Should().BeTrue();
                Volatile.Read(ref openEvents).Should().Be(0);
                Volatile.Read(ref closeEvents).Should().Be(0);
            }
        }
        finally
        {
            releaseSettings.TrySetResult(true);
            releaseStop.TrySetResult(true);
            Log.Reset();
        }
    }

    [Test]
    public async Task AgentClient_Stop_During_The_Handshake_Should_Keep_Settings_Off_The_Wire()
    {
        var upgradeRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeUpgrade = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(beforeUpgrade: async () =>
        {
            upgradeRequested.TrySetResult(true);
            await completeUpgrade.Task;
        });
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Agent.v2.WebSocket.Client(apiKey, options);

        var logs = new SignalingLoggerProvider();
        var releaseStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(logs);
        }));
        try
        {
            using var connectCts = new CancellationTokenSource(GateTimeout * 3);
            var connect = client.Connect(new AgentModels.SettingsSchema(), connectCts);
            await upgradeRequested.Task.WaitAsync(GateTimeout);

            // A Stop() marks the connection stopping during the handshake and is held right after.
            const string stopBegun = "Using default disconnect cancellation token";
            var stopHeld = logs.WhenLogged(stopBegun);
            logs.HoldWhenLogged(stopBegun, releaseStop.Task);
            var stop = Task.Run(() => client.Stop());
            await stopHeld.WaitAsync(GateTimeout);

            // The handshake completes on a connection that is already stopping.
            completeUpgrade.TrySetResult(true);
            (await connect.WaitAsync(GateTimeout)).Should().BeFalse();

            releaseStop.TrySetResult(true);
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();

            var connection = await server.WaitForConnectionAsync(1).WaitAsync(GateTimeout);
            await connection.Closed.WaitAsync(GateTimeout);
            server.DataFrames.Should().BeEmpty("the Settings step checks for a Stop() before it sends");
        }
        finally
        {
            completeUpgrade.TrySetResult(true);
            releaseStop.TrySetResult(true);
            Log.Reset();
        }
    }

    /// <summary>A log provider whose loggers throw when a log entry contains any of the given texts.</summary>
    private sealed class ThrowingLoggerProvider(params string[] texts) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new ThrowingLogger(texts);

        public void Dispose() { }

        private sealed class ThrowingLogger(string[] texts) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                if (texts.Any(message.Contains))
                {
                    throw new InvalidOperationException("Simulated log provider failure");
                }
            }
        }
    }

    [Test]
    public async Task Stop_Should_Release_The_Connection_When_A_Log_Provider_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            // It also throws on the error log in Stop()'s catch block, as a full disk would.
            builder.AddProvider(new ThrowingLoggerProvider("Using default disconnect cancellation token", " thrown "));
        }));
        try
        {
            // The provider throws inside Stop(), after the connection was marked stopping.
            Func<Task> stop = () => client.Stop().WaitAsync(GateTimeout);
            await stop.Should().ThrowAsync<InvalidOperationException>();
        }
        finally
        {
            Log.Reset();
        }

        // The stopping connection was still released, so the client can connect again.
        using var connectCts = new CancellationTokenSource(GateTimeout);
        var reconnected = await client.Connect(server.Endpoint, connectCts).WaitAsync(GateTimeout * 2);

        using (new AssertionScope())
        {
            reconnected.Should().BeTrue("a Stop() that throws must still release its connection");
            server.AcceptedConnections.Should().Be(2);
        }

        await client.Stop();
    }

    /// <summary>
    /// A loopback server for the close-message tests: it signals <paramref name="firstReceived"/>
    /// when the first message of the given type arrives, and closes the connection on a second one.
    /// </summary>
    private static LoopbackWebSocketServer ServerClosingOnSecond(string messageType, TaskCompletionSource<bool> firstReceived) => new(async connection =>
    {
        var seen = 0;
        while (true)
        {
            var frame = await connection.ReadFrameAsync();
            if (frame.IsText && frame.Text.Contains($"\"{messageType}\"") && ++seen == 1)
            {
                firstReceived.TrySetResult(true);
            }
            else if (frame.IsText && frame.Text.Contains($"\"{messageType}\""))
            {
                await connection.SendCloseAsync();
                return;
            }
        }
    });

    [Test]
    public async Task FluxClient_Connect_While_Connected_Should_Not_Let_Stop_Resend_CloseStream()
    {
        var firstCloseStream = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = ServerClosingOnSecond("CloseStream", firstCloseStream);
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.WebSocket.Client(apiKey, options);
        var schema = new FluxSchema { Model = "flux-general-en" };
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        // CloseStream goes out; the caller gives up on the grace period once the server has it.
        using var closeToken = new CancellationTokenSource();
        var sendClose = client.SendClose(false, closeToken);
        await firstCloseStream.Task.WaitAsync(GateTimeout);
        closeToken.Cancel();
        await sendClose.WaitAsync(GateTimeout);

        // A Connect() that returns the current connection must not re-arm CloseStream for it.
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        using (new AssertionScope())
        {
            server.AcceptedConnections.Should().Be(1);
            server.DataFrames.Count(f => f.IsText && f.Text.Contains("\"CloseStream\"")).Should().Be(1,
                "CloseStream is sent at most once per connection");
        }
    }

    [Test]
    public async Task FluxSpeakClient_Connect_While_Connected_Should_Not_Let_Stop_Resend_Close()
    {
        var firstClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = ServerClosingOnSecond("Close", firstClose);
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, options);
        var schema = new FluxSpeak.SpeakSchema { Model = "flux-alexis-en" };
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        // Close goes out; the caller gives up on the grace period once the server has it.
        using var closeToken = new CancellationTokenSource();
        var sendClose = client.SendClose(false, closeToken);
        await firstClose.Task.WaitAsync(GateTimeout);
        closeToken.Cancel();
        await sendClose.WaitAsync(GateTimeout);

        // A Connect() that returns the current connection must not re-arm Close for it.
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        using (new AssertionScope())
        {
            server.AcceptedConnections.Should().Be(1);
            server.DataFrames.Count(f => f.IsText && f.Text.Contains("\"Close\"")).Should().Be(1,
                "Close is sent at most once per connection");
        }
    }

    /// <summary>
    /// A Listen client that exposes its current connection and holds its sender inside the send of
    /// a marker message until the test opens <see cref="ReleaseSender"/>.
    /// </summary>
    private sealed class FinalizeListenClient : Deepgram.Clients.Listen.v2.WebSocket.Client
    {
        public static readonly byte[] Marker = { 0xC2, 0xC2 };

        public FinalizeListenClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal ConnectionState? Current => CurrentConnection;

        public TaskCompletionSource<bool> SenderBlocked { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseSender { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            if (data.Count == Marker.Length && data.SequenceEqual(Marker))
            {
                SenderBlocked.TrySetResult(true);
                await ReleaseSender.Task;
            }
            await base.SendOnConnection(owned, data, messageType, token);
        }
    }

    [Test]
    public async Task ListenClient_Finalize_For_A_Stopped_Connection_Should_Not_Wait_On_A_Newer_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new FinalizeListenClient(apiKey, options);
        var schema = new ListenModels.LiveSchema { Model = "nova-3" };

        // Connection A, as an autoflush loop holds it.
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();
        var connectionA = client.Current!;
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        // Connection B, whose sender is busy with queued audio.
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();
        client.SendBinary(FinalizeListenClient.Marker);
        await client.SenderBlocked.Task.WaitAsync(GateTimeout);

        try
        {
            // A's autoflush Finalize must not wait for B's queue.
            Func<Task> finalize = () => client.SendFinalize(connectionA).WaitAsync(GateTimeout);
            await finalize.Should().NotThrowAsync("a Finalize for a stopped connection must not wait on a newer connection's sender");
        }
        finally
        {
            client.ReleaseSender.TrySetResult(true);
        }

        // A sentinel queued on B: everything sent on B before it is on the wire when it arrives.
        client.SendMessage(Encoding.UTF8.GetBytes("{\"type\":\"Sentinel\"}"));
        var connectionB = await server.WaitForConnectionAsync(2).WaitAsync(GateTimeout);
        var before = new List<string>();
        while (true)
        {
            var frame = await connectionB.ReadFrameAsync().WaitAsync(GateTimeout);
            if (frame.IsText && frame.Text.Contains("\"Sentinel\""))
            {
                break;
            }
            if (frame.IsText)
            {
                before.Add(frame.Text);
            }
        }

        before.Should().NotContain(f => f.Contains("\"Finalize\""), "connection A's Finalize must not reach connection B");

        await client.Stop();
    }

    /// <summary>A Speak client that exposes its current connection.</summary>
    private sealed class ExposedSpeakClient : Deepgram.Clients.Speak.v2.WebSocket.Client
    {
        public ExposedSpeakClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal ConnectionState? Current => CurrentConnection;
    }

    [Test]
    public async Task SpeakClient_AutoFlush_For_A_Stopped_Connection_Should_Not_Reach_A_Newer_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new ExposedSpeakClient(apiKey, options);
        var schema = new SpeakModels.SpeakSchema();

        // Connection A, as an autoflush loop holds it.
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();
        var connectionA = client.Current!;
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();

        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        // A's autoflush fires after A has ended, then the app speaks on connection B.
        client.SendAutoFlush(connectionA);
        client.SpeakWithText("sentinel");

        var connectionB = await server.WaitForConnectionAsync(2).WaitAsync(GateTimeout);
        var before = new List<string>();
        while (true)
        {
            var frame = await connectionB.ReadFrameAsync().WaitAsync(GateTimeout);
            if (frame.Text.Contains("sentinel"))
            {
                break;
            }
            before.Add(frame.Text);
        }

        before.Should().NotContain(f => f.Contains("\"Flush\""), "a Flush from connection A's autoflush must not reach connection B");

        await client.Stop();
    }

    /// <summary>
    /// Installs a log provider that throws on the given texts, runs <paramref name="act"/> (which
    /// must throw), runs <paramref name="afterAct"/>, resets the log, then checks that the client
    /// can connect again within the gate timeout.
    /// </summary>
    private static async Task AssertReleasedAfterThrowingLog(LoopbackWebSocketServer server,
        Func<CancellationTokenSource, Task<bool>> reconnect, Func<Task> act, string[] texts, Action? afterAct = null)
    {
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(new ThrowingLoggerProvider(texts));
        }));
        try
        {
            await act.Should().ThrowAsync<InvalidOperationException>();
            afterAct?.Invoke();
        }
        finally
        {
            Log.Reset();
        }

        using var reconnectCts = new CancellationTokenSource(GateTimeout);
        var reconnected = await reconnect(reconnectCts).WaitAsync(GateTimeout * 2);
        using (new AssertionScope())
        {
            reconnected.Should().BeTrue("a connection whose teardown or connect threw in a log call must still be released");
            server.AcceptedConnections.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Test]
    public async Task Connect_Should_Release_The_Connection_When_A_Log_Provider_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());

        // The provider throws during the handshake, and again on the error log in Connect()'s catch.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(server.Endpoint, cts),
            () => client.Connect(server.Endpoint).WaitAsync(GateTimeout),
            new[] { "Connecting to Deepgram API", " thrown " });

        client.IsConnected().Should().BeTrue();
        await client.Stop();
    }

    [Test]
    public async Task Connect_Should_Release_The_Claim_When_A_Log_Provider_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());

        // The provider throws right after the claim, before the handshake starts.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(server.Endpoint, cts),
            () => client.Connect(server.Endpoint).WaitAsync(GateTimeout),
            new[] { "Connection claimed" });

        using (new AssertionScope())
        {
            client.IsConnected().Should().BeTrue();
            server.AcceptedConnections.Should().Be(1, "the claim that threw never started a handshake");
        }
        await client.Stop();
    }

    [Test]
    public async Task FluxClient_Stop_Should_Release_The_Connection_When_A_Log_Provider_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.WebSocket.Client(apiKey, options);
        var schema = new FluxSchema { Model = "flux-general-en" };
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        // The provider throws while Stop() sends CloseStream, and again on the error log in its catch.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(schema, cts),
            () => client.Stop().WaitAsync(GateTimeout),
            new[] { "Sending Close Message Immediately", " thrown " });

        server.AcceptedConnections.Should().Be(2);
        await client.Stop();
    }

    [Test]
    public async Task FluxSpeakClient_Stop_Should_Release_The_Connection_When_A_Log_Provider_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, keepAlive: false, onPrem: true);
        using var client = new Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, options);
        var schema = new FluxSpeak.SpeakSchema { Model = "flux-alexis-en" };
        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        // The provider throws while Stop() sends Close, and again on the error log in its catch.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(schema, cts),
            () => client.Stop().WaitAsync(GateTimeout),
            new[] { "Sending Close Message Immediately", " thrown " });

        server.AcceptedConnections.Should().Be(2);
        await client.Stop();
    }

    [Test]
    public async Task Connect_Should_Raise_Neither_Open_Nor_Close_When_Logging_Throws_Before_Open()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        // The provider throws on the log line that announces Open delivery.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(server.Endpoint, cts),
            () => client.Connect(server.Endpoint).WaitAsync(GateTimeout),
            new[] { "Sending OpenResponse event" },
            () =>
            {
                using (new AssertionScope())
                {
                    Volatile.Read(ref openEvents).Should().Be(0, "the connection never raised Open");
                    Volatile.Read(ref closeEvents).Should().Be(0, "Close pairs with Open");
                }
            });

        await client.Stop();
    }

    [Test]
    public async Task Connect_Should_Raise_Close_When_Logging_Throws_After_Open()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => Interlocked.Increment(ref openEvents)));
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));

        // The provider throws after the Open handlers ran, before the receive loop starts.
        await AssertReleasedAfterThrowingLog(server,
            cts => client.Connect(server.Endpoint, cts),
            () => client.Connect(server.Endpoint).WaitAsync(GateTimeout),
            new[] { "Starting Receiver Thread" },
            () =>
            {
                using (new AssertionScope())
                {
                    Volatile.Read(ref openEvents).Should().Be(1);
                    Volatile.Read(ref closeEvents).Should().Be(1, "the connection raised Open, so its release raises Close");
                }
            });

        await client.Stop();
    }

    [Test]
    public async Task Open_Should_Reach_Every_Handler_When_Logging_A_Handler_Failure_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ExposedClient(NewApiKey());
        var secondHandlerRan = false;
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => throw new InvalidOperationException("Open handler failure")));
        await client.Subscribe(new EventHandler<Common.OpenResponse>((_, _) => secondHandlerRan = true));

        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(new ThrowingLoggerProvider("Exception occurred in Open event handler"));
        }));
        bool connected;
        try
        {
            connected = await client.Connect(server.Endpoint).WaitAsync(GateTimeout);
        }
        finally
        {
            Log.Reset();
        }

        using (new AssertionScope())
        {
            connected.Should().BeTrue();
            secondHandlerRan.Should().BeTrue("a failing log provider must not stop Open delivery");
        }

        await client.Stop();
    }
}
