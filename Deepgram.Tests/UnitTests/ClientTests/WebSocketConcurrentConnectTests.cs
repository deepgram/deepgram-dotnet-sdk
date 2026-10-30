// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using AgentModels = Deepgram.Models.Agent.v2.WebSocket;
using FluxModels = Deepgram.Models.Flux.WebSocket;
using FluxSpeakModels = Deepgram.Models.Flux.Speak.WebSocket;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using SpeakModels = Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Connect() while another connection is being released (#455, review B2). A released connection
/// stays published until its release has unpublished it; Connect() must wait for that instead of
/// reporting the dying connection as "already initialized". Each test pauses a release from a
/// cancellation callback on the connection's token: the release cancels that token first, before
/// it unpublishes, and it does so outside the lifecycle lock, so the callback holds the release in
/// exactly that window. Steps are ordered with TaskCompletionSource gates, never with delays.
/// </summary>
[NonParallelizable]
public class WebSocketConcurrentConnectTests
{
    // Upper bound for any single gate; reached only when a test is about to fail.
    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

    private static string NewApiKey() => new Faker().Random.Guid().ToString();

    private static DeepgramWsClientOptions Options(string apiKey, string endpoint) =>
        new(apiKey, endpoint, keepAlive: false, onPrem: true);

    /// <summary>Gives a test the token of the connection the client currently holds.</summary>
    public interface IExposesConnection
    {
        CancellationToken CurrentToken { get; }
    }

    /// <summary>
    /// Holds the release of one connection right after it cancelled the connection's token and
    /// before it unpublishes the connection, until <see cref="Open"/> is called.
    /// </summary>
    private sealed class ReleaseGate : IDisposable
    {
        private readonly TaskCompletionSource<bool> _proceed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenRegistration _registration;

        public ReleaseGate(CancellationToken connectionToken)
        {
            _registration = connectionToken.Register(() =>
            {
                Entered.TrySetResult(true);
                _proceed.Task.Wait(GateTimeout);
            });
        }

        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Open() => _proceed.TrySetResult(true);

        public void Dispose()
        {
            // Open before disposing: disposing a registration blocks while its callback runs.
            Open();
            _registration.Dispose();
        }
    }

    private sealed class BaseClient : AbstractWebSocketClient, IExposesConnection
    {
        public BaseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;

        public void AbortCurrentSocket() => CurrentConnection!.Socket.Abort();

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;
    }

    private sealed class ListenClient : Deepgram.Clients.Listen.v2.WebSocket.Client, IExposesConnection
    {
        public ListenClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class SpeakClient : Deepgram.Clients.Speak.v2.WebSocket.Client, IExposesConnection
    {
        public SpeakClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class AgentClient : Deepgram.Clients.Agent.v2.WebSocket.Client, IExposesConnection
    {
        public AgentClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class FluxClient : Deepgram.Clients.Flux.WebSocket.Client, IExposesConnection
    {
        public FluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class FluxSpeakClient : Deepgram.Clients.Flux.Speak.WebSocket.Client, IExposesConnection
    {
        public FluxSpeakClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    /// <summary>Drives one kind of v2 WebSocket client.</summary>
    public sealed record ClientKind(
        string Name,
        Func<string, AbstractWebSocketClient> Create,
        Func<AbstractWebSocketClient, string, Task<bool>> Connect,
        Func<AbstractWebSocketClient, Task<bool>> Stop,
        string? ServerClosesOn = null)
    {
        public override string ToString() => Name;
    }

    private static readonly ClientKind[] Kinds =
    {
        new("Base", _ => new BaseClient(NewApiKey()), (c, endpoint) => c.Connect(endpoint), c => c.Stop()),
        new("Listen",
            endpoint => { var key = NewApiKey(); return new ListenClient(key, Options(key, endpoint)); },
            (c, _) => ((ListenClient)c).Connect(new ListenModels.LiveSchema { Model = "nova-3" }),
            c => ((ListenClient)c).Stop()),
        new("Speak",
            endpoint => { var key = NewApiKey(); return new SpeakClient(key, Options(key, endpoint)); },
            (c, _) => ((SpeakClient)c).Connect(new SpeakModels.SpeakSchema()),
            c => ((SpeakClient)c).Stop()),
        // The Flux STT/TTS servers close on CloseStream/Close, as the services do, so the release
        // runs promptly instead of after the close grace period.
        new("FluxSTT",
            endpoint => { var key = NewApiKey(); return new FluxClient(key, Options(key, endpoint)); },
            (c, _) => ((FluxClient)c).Connect(new FluxModels.FluxSchema { Model = "flux-general-en" }),
            c => ((FluxClient)c).Stop(),
            "CloseStream"),
        new("FluxTTS",
            endpoint => { var key = NewApiKey(); return new FluxSpeakClient(key, Options(key, endpoint)); },
            (c, _) => ((FluxSpeakClient)c).Connect(new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" }),
            c => ((FluxSpeakClient)c).Stop(),
            "Close"),
    };

    public static IEnumerable<TestCaseData> ReleasingKinds() =>
        Kinds.Select(kind => new TestCaseData(kind.Name)
            .SetName($"Connect_During_A_Release_Should_Wait_And_Open_A_New_Connection_{kind.Name}"));

    private static LoopbackWebSocketServer ServerFor(ClientKind kind) => kind.ServerClosesOn is null
        ? new LoopbackWebSocketServer()
        : new LoopbackWebSocketServer(async connection =>
        {
            while (true)
            {
                var frame = await connection.ReadFrameAsync();
                if (frame.IsText && frame.Text.Contains($"\"{kind.ServerClosesOn}\""))
                {
                    await connection.SendCloseAsync();
                    return;
                }
            }
        });

    [TestCaseSource(nameof(ReleasingKinds))]
    public async Task Connect_During_A_Release_Should_Wait_And_Open_A_New_Connection(string kindName)
    {
        var kind = Kinds.Single(k => k.Name == kindName);
        using var server = ServerFor(kind);
        using var client = kind.Create(server.Endpoint);

        (await kind.Connect(client, server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        // Stop() runs off the test thread: its release blocks inside the gate.
        Task<bool> stop;
        using (var gate = new ReleaseGate(((IExposesConnection)client).CurrentToken))
        {
            stop = Task.Run(() => kind.Stop(client));
            await gate.Entered.Task.WaitAsync(GateTimeout);

            // Connection 1 is released but still published.
            var connect = kind.Connect(client, server.Endpoint);
            using (new AssertionScope())
            {
                connect.IsCompleted.Should().BeFalse("Connect() must wait for the release instead of reporting the dying connection");
                server.AcceptedConnections.Should().Be(1);
            }

            gate.Open();
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeTrue();
                server.AcceptedConnections.Should().Be(2, "Connect() opens a new connection once the old one is released");
                client.IsConnected().Should().BeTrue();
            }
        }

        (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        await kind.Stop(client).WaitAsync(GateTimeout);
    }

    [Test]
    public async Task AgentClient_Connect_During_A_Release_Should_Send_Settings_On_The_New_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        using var client = new AgentClient(apiKey, Options(apiKey, server.Endpoint));
        var settings = new AgentModels.SettingsSchema();

        (await client.Connect(settings).WaitAsync(GateTimeout)).Should().BeTrue();

        using (var gate = new ReleaseGate(client.CurrentToken))
        {
            var stop = Task.Run(() => client.Stop());
            await gate.Entered.Task.WaitAsync(GateTimeout);

            var connect = client.Connect(settings);
            connect.IsCompleted.Should().BeFalse();

            gate.Open();
            (await connect.WaitAsync(GateTimeout)).Should().BeTrue();
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }

        var second = await server.WaitForConnectionAsync(2).WaitAsync(GateTimeout);
        var firstFrame = await second.ReadFrameAsync().WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            server.AcceptedConnections.Should().Be(2);
            firstFrame.Text.Should().Contain("\"Settings\"", "the new connection is configured by its own Settings message");
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_During_A_Server_Close_Teardown_Should_Open_A_New_Connection()
    {
        var serverClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(async connection =>
        {
            if (connection.Number == 1)
            {
                await serverClose.Task;
                await connection.SendCloseAsync();
            }
        });
        using var client = new BaseClient(NewApiKey());

        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        using (var gate = new ReleaseGate(client.CurrentToken))
        {
            // The receive loop sees the server's close and starts the release, which the gate holds.
            serverClose.TrySetResult(true);
            await gate.Entered.Task.WaitAsync(GateTimeout);

            var connect = client.Connect(server.Endpoint);
            connect.IsCompleted.Should().BeFalse();

            gate.Open();
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeTrue();
                server.AcceptedConnections.Should().Be(2);
                client.IsConnected().Should().BeTrue();
            }
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_Waiting_For_A_Release_Should_Honor_Its_Token()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BaseClient(NewApiKey());

        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        using (var gate = new ReleaseGate(client.CurrentToken))
        {
            var stop = Task.Run(() => client.Stop());
            await gate.Entered.Task.WaitAsync(GateTimeout);

            using var connectToken = new CancellationTokenSource();
            var connect = client.Connect(server.Endpoint, connectToken);
            connect.IsCompleted.Should().BeFalse();

            // The caller gives up while the release is still held.
            connectToken.Cancel();
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse("the connect token expired before the previous connection was released");
                server.AcceptedConnections.Should().Be(1);
            }

            gate.Open();
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
    }

    [Test]
    public async Task Dispose_While_Connect_Waits_For_A_Release_Should_Fail_The_Connect()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BaseClient(NewApiKey());

        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        using (var gate = new ReleaseGate(client.CurrentToken))
        {
            var stop = Task.Run(() => client.Stop());
            await gate.Entered.Task.WaitAsync(GateTimeout);

            var connect = client.Connect(server.Endpoint);
            connect.IsCompleted.Should().BeFalse();

            client.Dispose();
            gate.Open();
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeFalse("the client was disposed while Connect() waited");
                server.AcceptedConnections.Should().Be(1);
            }

            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
    }

    /// <summary>
    /// A base client whose first SendClose() is held until the test opens <see cref="ReleaseSendClose"/>.
    /// </summary>
    private sealed class GatedCloseClient : AbstractWebSocketClient
    {
        private int _calls;

        public GatedCloseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public TaskCompletionSource<bool> SendCloseEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseSendClose { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
        {
            if (Interlocked.Increment(ref _calls) == 1)
            {
                SendCloseEntered.TrySetResult(true);
                await ReleaseSendClose.Task;
            }
        }
    }

    private static bool IsText(ArraySegment<byte> data, WebSocketMessageType messageType, string type) =>
        messageType == WebSocketMessageType.Text
        && Encoding.UTF8.GetString(data.Array!, data.Offset, data.Count).Contains($"\"{type}\"");

    /// <summary>
    /// A Flux STT client that, once its CloseStream message is on the wire, holds the sending Stop()
    /// until the test opens <see cref="ReleaseStop"/>: the Stop() is then inside its close grace period.
    /// </summary>
    private sealed class GatedCloseFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        public GatedCloseFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public TaskCompletionSource<bool> CloseSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseStop { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token, bool allowStopping = false)
        {
            await base.SendOnConnection(owned, data, messageType, token, allowStopping);
            if (IsText(data, messageType, "CloseStream") && CloseSent.TrySetResult(true))
            {
                await ReleaseStop.Task;
            }
        }
    }

    /// <summary>The Flux TTS twin of <see cref="GatedCloseFluxClient"/>: it holds after Close.</summary>
    private sealed class GatedCloseFluxSpeakClient : Deepgram.Clients.Flux.Speak.WebSocket.Client
    {
        public GatedCloseFluxSpeakClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public TaskCompletionSource<bool> CloseSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseStop { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal override async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token, bool allowStopping = false)
        {
            await base.SendOnConnection(owned, data, messageType, token, allowStopping);
            if (IsText(data, messageType, "Close") && CloseSent.TrySetResult(true))
            {
                await ReleaseStop.Task;
            }
        }
    }

    /// <summary>A Flux STT client whose CloseStream send fails with an unexpected exception.</summary>
    private sealed class ThrowingCloseFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        public ThrowingCloseFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal override Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token, bool allowStopping = false) =>
            IsText(data, messageType, "CloseStream")
                ? throw new InvalidOperationException("Simulated CloseStream failure")
                : base.SendOnConnection(owned, data, messageType, token, allowStopping);
    }

    /// <summary>A Flux TTS client whose Close send fails with an unexpected exception.</summary>
    private sealed class ThrowingCloseFluxSpeakClient : Deepgram.Clients.Flux.Speak.WebSocket.Client
    {
        public ThrowingCloseFluxSpeakClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        internal override Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token, bool allowStopping = false) =>
            IsText(data, messageType, "Close")
                ? throw new InvalidOperationException("Simulated Close failure")
                : base.SendOnConnection(owned, data, messageType, token, allowStopping);
    }

    [Test]
    public async Task Connect_During_Another_Handshake_Should_Wait_For_It()
    {
        var upgradeRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeUpgrade = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(beforeUpgrade: async () =>
        {
            upgradeRequested.TrySetResult(true);
            await completeUpgrade.Task;
        });
        using var client = new BaseClient(NewApiKey());

        var firstConnect = client.Connect(server.Endpoint);
        await upgradeRequested.Task.WaitAsync(GateTimeout);

        try
        {
            var secondConnect = client.Connect(server.Endpoint);
            secondConnect.IsCompleted.Should().BeFalse("a connection still in its handshake is not ready");

            completeUpgrade.TrySetResult(true);
            var firstConnected = await firstConnect.WaitAsync(GateTimeout);
            var secondConnected = await secondConnect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                firstConnected.Should().BeTrue();
                secondConnected.Should().BeTrue("the second Connect() returns true once the first connection is open");
                server.AcceptedConnections.Should().Be(1);
                client.IsConnected().Should().BeTrue();
            }
        }
        finally
        {
            completeUpgrade.TrySetResult(true);
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_During_A_Cancelled_Handshake_Should_Open_Its_Own_Connection()
    {
        // Hold only the first upgrade; later ones complete at once.
        var upgrades = 0;
        var firstUpgradeRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completeFirstUpgrade = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(beforeUpgrade: async () =>
        {
            if (Interlocked.Increment(ref upgrades) == 1)
            {
                firstUpgradeRequested.TrySetResult(true);
                await completeFirstUpgrade.Task;
            }
        });
        using var client = new BaseClient(NewApiKey());

        try
        {
            using var firstToken = new CancellationTokenSource();
            var firstConnect = client.Connect(server.Endpoint, firstToken);
            await firstUpgradeRequested.Task.WaitAsync(GateTimeout);

            var secondConnect = client.Connect(server.Endpoint);
            secondConnect.IsCompleted.Should().BeFalse();

            // The first Connect() gives up during its handshake.
            firstToken.Cancel();
            await ((Task)firstConnect).ContinueWith(_ => { }).WaitAsync(GateTimeout);
            var secondConnected = await secondConnect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                (firstConnect.IsCompletedSuccessfully && firstConnect.Result).Should().BeFalse("the first handshake was cancelled");
                secondConnected.Should().BeTrue("the second Connect() opens its own connection once the first is released");
                client.IsConnected().Should().BeTrue();
                // The first upgrade is still held by the server, so the one upgraded connection is
                // the second Connect()'s own.
                server.AcceptedConnections.Should().Be(1);
            }
        }
        finally
        {
            completeFirstUpgrade.TrySetResult(true);
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_During_Stop_Should_Wait_For_The_Release()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new GatedCloseClient(NewApiKey());

        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        // A Stop() that has begun but not yet released the connection.
        var stop = Task.Run(() => client.Stop());
        await client.SendCloseEntered.Task.WaitAsync(GateTimeout);

        try
        {
            var connect = client.Connect(server.Endpoint);
            using (new AssertionScope())
            {
                connect.IsCompleted.Should().BeFalse("a connection being stopped is not reported as connected");
                server.AcceptedConnections.Should().Be(1);
            }

            client.ReleaseSendClose.TrySetResult(true);
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeTrue();
                server.AcceptedConnections.Should().Be(2, "Connect() opens a new connection once the stopped one is released");
                client.IsConnected().Should().BeTrue();
            }
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
        finally
        {
            client.ReleaseSendClose.TrySetResult(true);
        }

        await client.Stop();
    }

    [Test]
    public async Task FluxClient_Connect_During_The_Grace_Period_Should_Wait()
    {
        // The server never closes, so the Stop() waits out its grace period after CloseStream.
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        using var client = new GatedCloseFluxClient(apiKey, Options(apiKey, server.Endpoint));
        var schema = new FluxModels.FluxSchema { Model = "flux-general-en" };

        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        var stop = Task.Run(() => client.Stop());
        await client.CloseSent.Task.WaitAsync(GateTimeout);

        try
        {
            // A budget that covers the grace period, so the outcome does not depend on timing.
            using var connectToken = new CancellationTokenSource(TimeSpan.FromTicks(GateTimeout.Ticks * 2));
            var connect = client.Connect(schema, connectToken);
            using (new AssertionScope())
            {
                connect.IsCompleted.Should().BeFalse("a Flux STT connection in its close grace period is not reported as connected");
                server.AcceptedConnections.Should().Be(1);
            }

            client.ReleaseStop.TrySetResult(true);
            var connected = await connect.WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));

            using (new AssertionScope())
            {
                connected.Should().BeTrue();
                server.AcceptedConnections.Should().Be(2);
                client.IsConnected().Should().BeTrue();
            }
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
        finally
        {
            client.ReleaseStop.TrySetResult(true);
        }

        // Skip the close grace period: the server never closes.
        using var expired = new CancellationTokenSource();
        expired.Cancel();
        await client.Stop(expired);
    }

    [Test]
    public async Task FluxSpeakClient_Connect_During_The_Grace_Period_Should_Wait()
    {
        // The server never closes, so the Stop() waits out its grace period after Close.
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        using var client = new GatedCloseFluxSpeakClient(apiKey, Options(apiKey, server.Endpoint));
        var schema = new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" };

        (await client.Connect(schema).WaitAsync(GateTimeout)).Should().BeTrue();

        var stop = Task.Run(() => client.Stop());
        await client.CloseSent.Task.WaitAsync(GateTimeout);

        try
        {
            using var connectToken = new CancellationTokenSource(TimeSpan.FromTicks(GateTimeout.Ticks * 2));
            var connect = client.Connect(schema, connectToken);
            using (new AssertionScope())
            {
                connect.IsCompleted.Should().BeFalse("a Flux TTS connection in its close grace period is not reported as connected");
                server.AcceptedConnections.Should().Be(1);
            }

            client.ReleaseStop.TrySetResult(true);
            var connected = await connect.WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));

            using (new AssertionScope())
            {
                connected.Should().BeTrue();
                server.AcceptedConnections.Should().Be(2);
                client.IsConnected().Should().BeTrue();
            }
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
        finally
        {
            client.ReleaseStop.TrySetResult(true);
        }

        // Skip the close grace period: the server never closes.
        using var expired = new CancellationTokenSource();
        expired.Cancel();
        await client.Stop(expired);
    }

    [Test]
    public async Task Connect_From_An_Open_Handler_Should_Return_True_Without_Waiting()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BaseClient(NewApiKey());
        var openEvents = 0;
        var innerReturned = false;
        var innerResult = false;
        await client.Subscribe(new EventHandler<Deepgram.Models.Common.v2.WebSocket.OpenResponse>((_, _) =>
        {
            if (Interlocked.Increment(ref openEvents) == 1)
            {
                // The connection is opened (Open is being delivered), so it is ready: no waiting.
                var inner = client.Connect(server.Endpoint);
                innerReturned = inner.Wait(GateTimeout);
                innerResult = innerReturned && inner.Result;
            }
        }));

        var connected = await client.Connect(server.Endpoint).WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));

        using (new AssertionScope())
        {
            innerReturned.Should().BeTrue("a Connect() from an Open handler must not wait for the connection it runs in");
            innerResult.Should().BeTrue();
            connected.Should().BeTrue();
            server.AcceptedConnections.Should().Be(1);
            Volatile.Read(ref openEvents).Should().Be(1);
        }

        await client.Stop();
    }

    [Test]
    public async Task Connect_From_An_Open_Handler_After_A_Drop_Should_Open_A_New_Connection()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new BaseClient(NewApiKey());
        var openEvents = 0;
        var closeEvents = 0;
        var innerReturned = false;
        var innerResult = false;
        await client.Subscribe(new EventHandler<Deepgram.Models.Common.v2.WebSocket.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        await client.Subscribe(new EventHandler<Deepgram.Models.Common.v2.WebSocket.OpenResponse>((_, _) =>
        {
            if (Interlocked.Increment(ref openEvents) == 1)
            {
                // The connection drops while the Open handler runs, before any receive loop could see
                // it. A Connect() with no timeout must not wait for a release nothing would start.
                client.AbortCurrentSocket();
                using var noTimeout = new CancellationTokenSource();
                var inner = client.Connect(server.Endpoint, noTimeout);
                innerReturned = inner.Wait(GateTimeout);
                innerResult = innerReturned && inner.Result;
            }
        }));

        var connected = await client.Connect(server.Endpoint).WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));

        using (new AssertionScope())
        {
            innerReturned.Should().BeTrue("Connect() stops the dropped connection itself instead of waiting");
            innerResult.Should().BeTrue();
            connected.Should().BeFalse("the connection this call opened was stopped before it returned");
            server.AcceptedConnections.Should().Be(2);
            client.IsConnected().Should().BeTrue("the inner Connect() opened a new connection");
            Volatile.Read(ref closeEvents).Should().Be(1, "the dropped connection raised Open, so it raises Close once");
        }

        await client.Stop();
    }

    [Test]
    public async Task FluxClient_Stop_Should_Release_When_The_Close_Send_Throws_Unexpectedly()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        using var client = new ThrowingCloseFluxClient(apiKey, Options(apiKey, server.Endpoint));
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<FluxModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        (await client.Connect(new FluxModels.FluxSchema { Model = "flux-general-en" }).WaitAsync(GateTimeout)).Should().BeTrue();

        Func<Task<bool>> stop = () => client.Stop().WaitAsync(GateTimeout);

        await stop.Should().ThrowAsync<InvalidOperationException>();
        using (new AssertionScope())
        {
            client.State().Should().Be(WebSocketState.None, "Stop() must release the connection even when it throws");
            Volatile.Read(ref closeEvents).Should().Be(1);
        }
    }

    [Test]
    public async Task FluxSpeakClient_Stop_Should_Release_When_The_Close_Send_Throws_Unexpectedly()
    {
        using var server = new LoopbackWebSocketServer();
        var apiKey = NewApiKey();
        using var client = new ThrowingCloseFluxSpeakClient(apiKey, Options(apiKey, server.Endpoint));
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<FluxSpeakModels.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        (await client.Connect(new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" }).WaitAsync(GateTimeout)).Should().BeTrue();

        Func<Task<bool>> stop = () => client.Stop().WaitAsync(GateTimeout);

        await stop.Should().ThrowAsync<InvalidOperationException>();
        using (new AssertionScope())
        {
            client.State().Should().Be(WebSocketState.None, "Stop() must release the connection even when it throws");
            Volatile.Read(ref closeEvents).Should().Be(1);
        }
    }
}
