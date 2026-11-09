// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using AgentModels = Deepgram.Models.Agent.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using FluxModels = Deepgram.Models.Flux.WebSocket;
using FluxSpeakModels = Deepgram.Models.Flux.Speak.WebSocket;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using SpeakModels = Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Stop() called from inside a Close handler (#455, review B1). A release unpublishes the
/// connection before it raises Close, so a Stop() from the handler finds no connection and returns
/// at once instead of waiting for the release that is delivering the event. Each test drives a
/// real ClientWebSocket against a loopback server and orders the steps with TaskCompletionSource
/// gates, never with delays.
/// </summary>
[NonParallelizable]
public class WebSocketCloseReentrancyTests
{
    // Upper bound for any single gate; reached only when a test is about to fail.
    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

    private static string NewApiKey() => new Faker().Random.Guid().ToString();

    public enum Trigger
    {
        UserStop,
        ServerClose,
    }

    /// <summary>
    /// Drives one kind of v2 WebSocket client: how to create, connect, stop and subscribe to Close,
    /// what its server does, and how a test makes that server close the connection.
    /// </summary>
    public abstract class ClientKind
    {
        public abstract string Name { get; }

        public abstract AbstractWebSocketClient Create(string endpoint);

        public abstract Task<bool> Connect(AbstractWebSocketClient client, string endpoint);

        public abstract Task<bool> Stop(AbstractWebSocketClient client);

        public abstract Task SubscribeClose(AbstractWebSocketClient client, Action onClose);

        /// <summary>
        /// The server side. By default it closes the connection when the test completes
        /// <paramref name="closeNow"/>.
        /// </summary>
        public virtual async Task ServerScript(LoopbackWebSocketServer.Connection connection, Task closeNow)
        {
            await closeNow;
            await connection.SendCloseAsync();
        }

        /// <summary>Makes the server close the connection.</summary>
        public virtual Task TriggerServerClose(AbstractWebSocketClient client, TaskCompletionSource<bool> closeNow)
        {
            closeNow.TrySetResult(true);
            return Task.CompletedTask;
        }

        public override string ToString() => Name;

        protected static DeepgramWsClientOptions Options(string apiKey, string endpoint) =>
            new(apiKey, endpoint, keepAlive: false, onPrem: true);
    }

    /// <summary>A base client whose SendClose() sends nothing.</summary>
    private sealed class BaseClient : AbstractWebSocketClient
    {
        public BaseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;
    }

    private sealed class BaseKind : ClientKind
    {
        public override string Name => "Base";

        public override AbstractWebSocketClient Create(string endpoint) => new BaseClient(NewApiKey());

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) => client.Connect(endpoint);

        public override Task<bool> Stop(AbstractWebSocketClient client) => client.Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => onClose()));
    }

    private sealed class ListenKind : ClientKind
    {
        public override string Name => "Listen";

        public override AbstractWebSocketClient Create(string endpoint)
        {
            var apiKey = NewApiKey();
            return new Deepgram.Clients.Listen.v2.WebSocket.Client(apiKey, Options(apiKey, endpoint));
        }

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) =>
            ((Deepgram.Clients.Listen.v2.WebSocket.Client)client).Connect(new ListenModels.LiveSchema { Model = "nova-3" });

        public override Task<bool> Stop(AbstractWebSocketClient client) =>
            ((Deepgram.Clients.Listen.v2.WebSocket.Client)client).Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            ((Deepgram.Clients.Listen.v2.WebSocket.Client)client).Subscribe(new EventHandler<ListenModels.CloseResponse>((_, _) => onClose()));
    }

    private sealed class SpeakKind : ClientKind
    {
        public override string Name => "Speak";

        public override AbstractWebSocketClient Create(string endpoint)
        {
            var apiKey = NewApiKey();
            return new Deepgram.Clients.Speak.v2.WebSocket.Client(apiKey, Options(apiKey, endpoint));
        }

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) =>
            ((Deepgram.Clients.Speak.v2.WebSocket.Client)client).Connect(new SpeakModels.SpeakSchema());

        public override Task<bool> Stop(AbstractWebSocketClient client) =>
            ((Deepgram.Clients.Speak.v2.WebSocket.Client)client).Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            ((Deepgram.Clients.Speak.v2.WebSocket.Client)client).Subscribe(new EventHandler<SpeakModels.CloseResponse>((_, _) => onClose()));
    }

    private sealed class AgentKind : ClientKind
    {
        public override string Name => "Agent";

        public override AbstractWebSocketClient Create(string endpoint)
        {
            var apiKey = NewApiKey();
            return new Deepgram.Clients.Agent.v2.WebSocket.Client(apiKey, Options(apiKey, endpoint));
        }

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) =>
            ((Deepgram.Clients.Agent.v2.WebSocket.Client)client).Connect(new AgentModels.SettingsSchema());

        public override Task<bool> Stop(AbstractWebSocketClient client) =>
            ((Deepgram.Clients.Agent.v2.WebSocket.Client)client).Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            ((Deepgram.Clients.Agent.v2.WebSocket.Client)client).Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => onClose()));

        /// <summary>The agent server closes only after it has received the Settings message.</summary>
        public override async Task ServerScript(LoopbackWebSocketServer.Connection connection, Task closeNow)
        {
            while (true)
            {
                var frame = await connection.ReadFrameAsync();
                if (frame.IsText && frame.Text.Contains("\"Settings\""))
                {
                    break;
                }
            }
            await closeNow;
            await connection.SendCloseAsync();
        }
    }

    /// <summary>
    /// The Flux STT server closes the connection when it receives CloseStream, as the service does.
    /// A test makes it close by sending CloseStream through the public SendClose().
    /// </summary>
    private sealed class FluxKind : ClientKind
    {
        public override string Name => "FluxSTT";

        public override AbstractWebSocketClient Create(string endpoint)
        {
            var apiKey = NewApiKey();
            return new Deepgram.Clients.Flux.WebSocket.Client(apiKey, Options(apiKey, endpoint));
        }

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) =>
            ((Deepgram.Clients.Flux.WebSocket.Client)client).Connect(new FluxModels.FluxSchema { Model = "flux-general-en" });

        public override Task<bool> Stop(AbstractWebSocketClient client) =>
            ((Deepgram.Clients.Flux.WebSocket.Client)client).Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            ((Deepgram.Clients.Flux.WebSocket.Client)client).Subscribe(new EventHandler<FluxModels.CloseResponse>((_, _) => onClose()));

        public override Task ServerScript(LoopbackWebSocketServer.Connection connection, Task closeNow) =>
            CloseOn(connection, "CloseStream");

        public override Task TriggerServerClose(AbstractWebSocketClient client, TaskCompletionSource<bool> closeNow) =>
            ((Deepgram.Clients.Flux.WebSocket.Client)client).SendClose();
    }

    /// <summary>The Flux TTS twin of <see cref="FluxKind"/>: the server closes on Close.</summary>
    private sealed class FluxSpeakKind : ClientKind
    {
        public override string Name => "FluxTTS";

        public override AbstractWebSocketClient Create(string endpoint)
        {
            var apiKey = NewApiKey();
            return new Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, Options(apiKey, endpoint));
        }

        public override Task<bool> Connect(AbstractWebSocketClient client, string endpoint) =>
            ((Deepgram.Clients.Flux.Speak.WebSocket.Client)client).Connect(new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" });

        public override Task<bool> Stop(AbstractWebSocketClient client) =>
            ((Deepgram.Clients.Flux.Speak.WebSocket.Client)client).Stop();

        public override Task SubscribeClose(AbstractWebSocketClient client, Action onClose) =>
            ((Deepgram.Clients.Flux.Speak.WebSocket.Client)client).Subscribe(new EventHandler<FluxSpeakModels.CloseResponse>((_, _) => onClose()));

        public override Task ServerScript(LoopbackWebSocketServer.Connection connection, Task closeNow) =>
            CloseOn(connection, "Close");

        public override Task TriggerServerClose(AbstractWebSocketClient client, TaskCompletionSource<bool> closeNow) =>
            ((Deepgram.Clients.Flux.Speak.WebSocket.Client)client).SendClose();
    }

    private static async Task CloseOn(LoopbackWebSocketServer.Connection connection, string messageType)
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
    }

    private static readonly ClientKind[] Kinds =
    {
        new BaseKind(), new ListenKind(), new SpeakKind(), new AgentKind(), new FluxKind(), new FluxSpeakKind(),
    };

    public static IEnumerable<TestCaseData> KindsAndTriggers() =>
        from kind in Kinds
        from trigger in new[] { Trigger.UserStop, Trigger.ServerClose }
        select new TestCaseData(kind, trigger)
            .SetName($"Close_Handler_Calling_Stop_Wait_Should_Not_Deadlock_{kind.Name}_{trigger}");

    [TestCaseSource(nameof(KindsAndTriggers))]
    public async Task Close_Handler_Calling_Stop_Wait_Should_Not_Deadlock(ClientKind kind, Trigger trigger)
    {
        var closeNow = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(connection => kind.ServerScript(connection, closeNow.Task));
        using var client = kind.Create(server.Endpoint);

        var closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeEvents = 0;
        var innerStopReturned = false;
        var innerStopResult = false;
        var stateInHandler = WebSocketState.Open;
        await kind.SubscribeClose(client, () =>
        {
            stateInHandler = client.State();

            // A blocking Stop() from inside the Close handler, as an app that cleans up on close would.
            var innerStop = kind.Stop(client);
            innerStopReturned = innerStop.Wait(GateTimeout);
            innerStopResult = innerStopReturned && innerStop.Result;

            Interlocked.Increment(ref closeEvents);
            closed.TrySetResult(true);
        });

        (await kind.Connect(client, server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        bool outerStopResult;
        if (trigger == Trigger.UserStop)
        {
            var outerStop = Task.Run(() => kind.Stop(client));
            await closed.Task.WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));
            outerStopResult = await outerStop.WaitAsync(GateTimeout);
        }
        else
        {
            var serverClose = Task.Run(() => kind.TriggerServerClose(client, closeNow));
            await closed.Task.WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));
            await serverClose.WaitAsync(GateTimeout);
            outerStopResult = await kind.Stop(client).WaitAsync(GateTimeout);
        }

        using (new AssertionScope())
        {
            innerStopReturned.Should().BeTrue("a Stop() from a Close handler must not wait for the release delivering Close");
            innerStopResult.Should().BeTrue();
            stateInHandler.Should().Be(WebSocketState.None, "the connection is unpublished before Close is raised");
            outerStopResult.Should().BeTrue();
            Volatile.Read(ref closeEvents).Should().Be(1);
        }
    }

    /// <summary>
    /// Holds the first SendClose() call until the test completes <see cref="Release"/>.
    /// </summary>
    private sealed class GatedCloseClient : AbstractWebSocketClient
    {
        private int _calls;

        public GatedCloseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
    public async Task Stop_From_A_Close_Handler_Should_Return_While_Another_Stop_Is_Pending()
    {
        // The server closes the connection when the test says so.
        var serverClose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new LoopbackWebSocketServer(async connection =>
        {
            await serverClose.Task;
            await connection.SendCloseAsync();
        });
        using var client = new GatedCloseClient(NewApiKey());
        var handlerStopReturned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var userStopDoneInHandler = true;
        Task<bool>? userStop = null;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) =>
        {
            // The user Stop() is still held in SendClose(): it has not returned.
            userStopDoneInHandler = userStop!.IsCompleted;
            var stop = client.Stop();
            handlerStopReturned.TrySetResult(stop.Wait(GateTimeout) && stop.Result);
        }));

        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // The user Stop() runs off the test's context and is held inside SendClose().
        userStop = Task.Run(() => client.Stop());
        await client.Entered.Task.WaitAsync(GateTimeout);

        // The server closes; the receive loop wins the release and raises Close, whose handler
        // calls Stop() while the user Stop() is still pending.
        serverClose.TrySetResult(true);
        bool handlerStop;
        try
        {
            handlerStop = await handlerStopReturned.Task.WaitAsync(TimeSpan.FromTicks(GateTimeout.Ticks * 2));
        }
        finally
        {
            client.Release.TrySetResult(true);
        }

        using (new AssertionScope())
        {
            handlerStop.Should().BeTrue("the handler's own Stop() finds no published connection and returns at once");
            userStopDoneInHandler.Should().BeFalse("the user Stop() was still pending while the handler ran");
            (await userStop.WaitAsync(GateTimeout)).Should().BeTrue();
        }
    }
}
