// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Abstractions.v2;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Microsoft.Extensions.Logging;
using AgentModels = Deepgram.Models.Agent.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using FluxModels = Deepgram.Models.Flux.WebSocket;
using FluxSpeakModels = Deepgram.Models.Flux.Speak.WebSocket;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
using SpeakModels = Deepgram.Models.Speak.v2.WebSocket;
using IExposesConnection = Deepgram.Tests.UnitTests.ClientTests.WebSocketConcurrentConnectTests.IExposesConnection;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// A teardown that loses the race to another teardown of the same connection waits for that
/// teardown's release without a bound, then for its Close delivery only up to its own token or a
/// fresh disconnect timeout, whichever comes first (#468). The disconnect timeout is test-owned
/// through <see cref="AbstractWebSocketClient.CloseDeliveryTimeoutFactory"/>, so no test waits out
/// 5 s: every bounded wait is observed as it starts, and the test ends it by cancelling the bound.
/// Steps are ordered with TaskCompletionSource gates, never with delays or log text.
/// </summary>
[NonParallelizable] // tests install a process-wide log provider
public class WebSocketTeardownBoundTests
{
    // Upper bound for any single gate; reached only when a test is about to fail.
    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

    // A test-owned bound fires on its own only after this, well past every gate.
    private static readonly TimeSpan BudgetSafetyNet = TimeSpan.FromSeconds(30);

    private static string NewApiKey() => new Faker().Random.Guid().ToString();

    private static DeepgramWsClientOptions Options(string apiKey, string endpoint) =>
        new(apiKey, endpoint, keepAlive: false, onPrem: true);

    private CapturingLoggerProvider _logs = null!;

    [SetUp]
    public void InstallLogs()
    {
        _logs = new CapturingLoggerProvider();
        Log.ResetAndConfigureForTests(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(_logs);
        }));
    }

    [TearDown]
    public void RemoveLogs()
    {
        Log.Reset();
        _logs.Dispose();
    }

    /// <summary>Records log entries; can throw from entries that match a test's predicate.</summary>
    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(MelLogLevel Level, string Message)> Entries { get; } = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _signals = new();

        public Func<MelLogLevel, string, bool>? ThrowWhen { get; set; }

        /// <summary>Completes when an entry containing <paramref name="text"/> is logged.</summary>
        public Task WhenLogged(string text) =>
            _signals.GetOrAdd(text, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)).Task;

        public bool HasWarning(string text) =>
            Entries.Any(e => e.Level == MelLogLevel.Warning && e.Message.Contains(text));

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

        public void Dispose() { }

        private sealed class CapturingLogger(CapturingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                provider.Entries.Enqueue((logLevel, message));
                foreach (var signal in provider._signals)
                {
                    if (message.Contains(signal.Key))
                    {
                        signal.Value.TrySetResult(true);
                    }
                }
                if (provider.ThrowWhen?.Invoke(logLevel, message) == true)
                {
                    throw new InvalidOperationException("Simulated log provider failure");
                }
            }
        }
    }

    /// <summary>
    /// The client's bound for each wait on another teardown's Close. Every bound is recorded with
    /// the waiting caller's token, so a test finds its own Stop()'s wait (the receive loop can be a
    /// losing teardown too) and ends it by cancelling the bound.
    /// </summary>
    private sealed class CloseDeliveryBudgets
    {
        private readonly object _gate = new();
        private readonly List<(CancellationToken Caller, CancellationTokenSource Budget)> _created = new();
        private readonly List<(CancellationToken Caller, TaskCompletionSource<CancellationTokenSource> Waiter)> _waiters = new();

        public void Attach(AbstractWebSocketClient client) => client.CloseDeliveryTimeoutFactory = Create;

        private CancellationTokenSource Create(CancellationToken caller)
        {
            var budget = new CancellationTokenSource(BudgetSafetyNet);
            List<TaskCompletionSource<CancellationTokenSource>> hits;
            lock (_gate)
            {
                _created.Add((caller, budget));
                hits = _waiters.Where(w => w.Caller.Equals(caller)).Select(w => w.Waiter).ToList();
            }
            foreach (var hit in hits)
            {
                hit.TrySetResult(budget);
            }
            return budget;
        }

        /// <summary>Completes when a bounded wait starts for the Stop() that holds this token.</summary>
        public Task<CancellationTokenSource> WhenCreatedFor(CancellationToken caller)
        {
            lock (_gate)
            {
                foreach (var created in _created)
                {
                    if (created.Caller.Equals(caller))
                    {
                        return Task.FromResult(created.Budget);
                    }
                }
                var waiter = new TaskCompletionSource<CancellationTokenSource>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waiters.Add((caller, waiter));
                return waiter.Task;
            }
        }

        public bool WasCreatedFor(CancellationToken caller)
        {
            lock (_gate)
            {
                return _created.Any(c => c.Caller.Equals(caller));
            }
        }
    }

    /// <summary>A base client whose SendClose() runs the test's script with its 1-based call number.</summary>
    private sealed class ScriptedClient : AbstractWebSocketClient, IExposesConnection
    {
        private int _calls;

        public ScriptedClient()
            : this(NewApiKey()) { }

        private ScriptedClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public Func<int, Task> Script { get; set; } = _ => Task.CompletedTask;

        /// <summary>The server this client connects to, for the client-kind matrix.</summary>
        public string? Endpoint { get; set; }

        public CancellationToken CurrentToken => CurrentConnection!.Token;

        /// <summary>Runs after Close has been delivered to every handler; a test can throw from it.</summary>
        public Action? AfterRaiseClose { get; set; }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Script(Interlocked.Increment(ref _calls));

        internal override void RaiseClose()
        {
            base.RaiseClose();
            AfterRaiseClose?.Invoke();
        }
    }

    /// <summary>
    /// Two Stop() calls on one connection. Stop A wins: its SendClose() is released first, it
    /// releases the connection and raises Close, and the Close handler blocks. Stop B entered
    /// SendClose() before A released and is let go only once the handler is running, so it finds
    /// the connection released by A and becomes the losing teardown.
    /// </summary>
    private sealed class RacingStops
    {
        private readonly TaskCompletionSource<bool> _firstEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _secondEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _releaseFirst = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _releaseSecond = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public RacingStops(ScriptedClient client, bool loserHandshakeTimesOut = false)
        {
            client.Script = async call =>
            {
                if (call == 1)
                {
                    _firstEntered.TrySetResult(true);
                    await _releaseFirst.Task.ConfigureAwait(false);
                }
                else if (call == 2)
                {
                    _secondEntered.TrySetResult(true);
                    await _releaseSecond.Task.ConfigureAwait(false);
                    if (loserHandshakeTimesOut)
                    {
                        // The loser's close handshake ran out of time: the timed-out Stop() path.
                        throw new OperationCanceledException();
                    }
                }
            };
        }

        public Task<bool> Winner { get; private set; } = null!;

        public Task<bool> Loser { get; private set; } = null!;

        public async Task Start(Func<Task<bool>> winner, Func<Task<bool>> loser, Task closeHandlerEntered)
        {
            Winner = winner();
            await _firstEntered.Task.WaitAsync(GateTimeout);
            Loser = loser();
            await _secondEntered.Task.WaitAsync(GateTimeout);

            _releaseFirst.TrySetResult(true);
            await closeHandlerEntered.WaitAsync(GateTimeout);
            _releaseSecond.TrySetResult(true);
        }
    }

    /// <summary>A Close handler that blocks the first time it runs, until the test opens it.</summary>
    private sealed class BlockingCloseHandler : IDisposable
    {
        private readonly ManualResetEventSlim _finish = new(false);
        private int _events;

        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Action? BeforeBlocking { get; set; }

        public Action? AfterBlocking { get; set; }

        public int Events => Volatile.Read(ref _events);

        public void Handle()
        {
            if (Interlocked.Increment(ref _events) != 1)
            {
                return;
            }
            BeforeBlocking?.Invoke();
            Entered.TrySetResult(true);
            _finish.Wait(BudgetSafetyNet);
            AfterBlocking?.Invoke();
        }

        public void Finish() => _finish.Set();

        public void Dispose()
        {
            _finish.Set();
            _finish.Dispose();
        }
    }

    public enum Bound
    {
        CallerToken,
        DisconnectTimeout,
    }

    [TestCase(Bound.CallerToken)]
    [TestCase(Bound.DisconnectTimeout)]
    public async Task Losing_Stop_With_A_Live_Token_Should_Return_When_The_First_Bound_Fires(Bound fires)
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // A caller token that would never fire on its own.
        using var callerCts = new CancellationTokenSource();
        var race = new RacingStops(client);
        await race.Start(
            () => Task.Run(() => client.Stop()),
            () => Task.Run(() => client.Stop(callerCts)),
            handler.Entered.Task);

        var budget = await budgets.WhenCreatedFor(callerCts.Token).WaitAsync(GateTimeout);
        race.Loser.IsCompleted.Should().BeFalse("the loser waits for Close while the handler runs");

        if (fires == Bound.CallerToken)
        {
            callerCts.Cancel();
        }
        else
        {
            budget.Cancel();
        }
        var loserResult = await race.Loser.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            loserResult.Should().BeTrue("the connection is released; only its Close handler is still running");
            race.Winner.IsCompleted.Should().BeFalse("the Stop() that raised Close still waits for its handler");
            _logs.HasWarning("is still running. Continuing without waiting").Should().BeTrue();
            client.IsConnected().Should().BeFalse();
        }

        handler.Finish();
        (await race.Winner.WaitAsync(GateTimeout)).Should().BeTrue();
        handler.Events.Should().Be(1, "one connection was stopped, so Close fires once");
    }

    [Test]
    public async Task Losing_Stop_With_An_Expired_Token_Should_Wait_For_Close_Up_To_A_Fresh_Disconnect_Timeout()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        using var expired = new CancellationTokenSource();
        expired.Cancel();
        var race = new RacingStops(client, loserHandshakeTimesOut: true);
        await race.Start(
            () => Task.Run(() => client.Stop()),
            () => Task.Run(() => client.Stop(expired)),
            handler.Entered.Task);

        // The expired token does not skip the wait: a fresh disconnect timeout bounds it.
        var budget = await budgets.WhenCreatedFor(expired.Token).WaitAsync(GateTimeout);
        race.Loser.IsCompleted.Should().BeFalse("an expired token must not skip the wait for Close");

        budget.Cancel();
        var loserResult = await race.Loser.WaitAsync(GateTimeout);

        using (new AssertionScope())
        {
            loserResult.Should().BeTrue();
            race.Winner.IsCompleted.Should().BeFalse();
            _logs.HasWarning("is still running. Continuing without waiting").Should().BeTrue();
        }

        handler.Finish();
        (await race.Winner.WaitAsync(GateTimeout)).Should().BeTrue();
        handler.Events.Should().Be(1);
    }

    [Test]
    public async Task Timed_Out_Stop_Should_Not_Return_Before_The_Winning_Teardown_Has_Released()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        var closeEvents = 0;
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => Interlocked.Increment(ref closeEvents)));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // Hold the winning release inside its first step (cancelling the connection's token),
        // before it unpublishes the connection.
        var inRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var holdRelease = new ManualResetEventSlim(false);
        var registration = client.CurrentToken.Register(() =>
        {
            inRelease.TrySetResult(true);
            holdRelease.Wait(GateTimeout);
        });
        try
        {
            var winner = Task.Run(() => client.Stop());
            await inRelease.Task.WaitAsync(GateTimeout);

            // Stop() runs synchronously up to its first wait, so once it returns its task, the
            // loser is already waiting for the held release.
            using var expired = new CancellationTokenSource();
            expired.Cancel();
            var loser = client.Stop(expired);

            using (new AssertionScope())
            {
                loser.IsCompleted.Should().BeFalse("Stop() never returns before its connection is released");
                budgets.WasCreatedFor(expired.Token).Should().BeFalse("the wait for the release has no bound");
            }

            holdRelease.Set();
            var results = await Task.WhenAll(winner, loser).WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                results.Should().AllBeEquivalentTo(true);
                Volatile.Read(ref closeEvents).Should().Be(1);
                client.IsConnected().Should().BeFalse();
            }
        }
        finally
        {
            // Open before disposing: disposing a registration blocks while its callback runs.
            holdRelease.Set();
            registration.Dispose();
        }
    }

    [Test]
    public async Task Stop_From_A_Close_Handler_Should_Return_At_Once_While_A_Loser_Waits()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        var innerStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.BeforeBlocking = () =>
        {
            var inner = client.Stop();
            innerStop.TrySetResult(inner.IsCompleted && inner.Result);
        };
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        using var callerCts = new CancellationTokenSource();
        var race = new RacingStops(client);
        await race.Start(
            () => Task.Run(() => client.Stop()),
            () => Task.Run(() => client.Stop(callerCts)),
            handler.Entered.Task);

        (await innerStop.Task.WaitAsync(GateTimeout)).Should()
            .BeTrue("a Stop() from the handler finds no published connection and returns at once");

        var budget = await budgets.WhenCreatedFor(callerCts.Token).WaitAsync(GateTimeout);
        race.Loser.IsCompleted.Should().BeFalse();
        budget.Cancel();
        (await race.Loser.WaitAsync(GateTimeout)).Should().BeTrue();

        handler.Finish();
        (await race.Winner.WaitAsync(GateTimeout)).Should().BeTrue();
        handler.Events.Should().Be(1);
    }

    /// <summary>A UI-like context whose only thread is blocked: posted callbacks never run.</summary>
    private sealed class BlockedThreadSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
        }

        public override void Send(SendOrPostCallback d, object? state) =>
            throw new NotSupportedException("The blocked thread cannot run callbacks.");
    }

    private static Task<bool> RunOnBlockedThreadAsync(Func<bool> call)
    {
        var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(new BlockedThreadSynchronizationContext());
                result.TrySetResult(call());
            }
            catch (Exception ex)
            {
                result.TrySetException(ex);
            }
        });
        thread.IsBackground = true;
        thread.Start();
        return result.Task;
    }

    [Test]
    public async Task Losing_Stop_Wait_On_A_Blocked_UI_Thread_Should_Return_Within_The_Bound()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        using var callerCts = new CancellationTokenSource();
        var race = new RacingStops(client);
        await race.Start(
            () => Task.Run(() => client.Stop()),
            () => RunOnBlockedThreadAsync(() => client.Stop(callerCts).GetAwaiter().GetResult()),
            handler.Entered.Task);

        var budget = await budgets.WhenCreatedFor(callerCts.Token).WaitAsync(GateTimeout);
        budget.Cancel();
        (await race.Loser.WaitAsync(GateTimeout)).Should()
            .BeTrue("every await on the bounded path uses ConfigureAwait(false)");

        handler.Finish();
        (await race.Winner.WaitAsync(GateTimeout)).Should().BeTrue();
    }

    [Test]
    public async Task Close_Delivery_Should_Complete_When_The_Close_Path_Throws()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        // Handler exceptions are isolated, so the throw comes from the Close path itself, after
        // every handler has returned.
        client.AfterRaiseClose = () => throw new InvalidOperationException("Close path failure");
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // Neither bound of the loser ever fires on its own within the test.
        using var callerCts = new CancellationTokenSource();
        var race = new RacingStops(client);
        await race.Start(
            () => Task.Run(() => client.Stop()),
            () => Task.Run(() => client.Stop(callerCts)),
            handler.Entered.Task);

        await budgets.WhenCreatedFor(callerCts.Token).WaitAsync(GateTimeout);
        race.Loser.IsCompleted.Should().BeFalse();

        handler.Finish();

        // The loser returns because Close delivery completed, not because a bound fired.
        (await race.Loser.WaitAsync(GateTimeout)).Should().BeTrue();
        await FluentActions.Awaiting(() => race.Winner.WaitAsync(GateTimeout))
            .Should().ThrowAsync<InvalidOperationException>("the Close path's exception escapes the winning Stop()");

        client.AfterRaiseClose = null;
        (await client.Connect(server.Endpoint).WaitAsync(GateTimeout)).Should()
            .BeTrue("a failed Close delivery must not wedge the next Connect()");
        server.AcceptedConnections.Should().Be(2);
        (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
    }

    [Test]
    public async Task Connect_Waiting_For_A_Previous_Connection_Should_Bound_Its_Wait_For_Close()
    {
        using var server = new LoopbackWebSocketServer();
        using var client = new ScriptedClient();
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await client.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => handler.Handle()));
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        // Hold the Stop()'s release before it unpublishes the connection, so the Connect() below
        // finds the old connection still published and waits for it.
        var inRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var holdRelease = new ManualResetEventSlim(false);
        var registration = client.CurrentToken.Register(() =>
        {
            inRelease.TrySetResult(true);
            holdRelease.Wait(GateTimeout);
        });
        try
        {
            var stop = Task.Run(() => client.Stop());
            await inRelease.Task.WaitAsync(GateTimeout);

            // A connect token that never fires on its own.
            using var connectCts = new CancellationTokenSource();
            // Logged only by a Connect() that found the old connection stopping or released.
            var connectWaiting = _logs.WhenLogged("Waiting for the previous connection to be released");
            var connect = Task.Run(() => client.Connect(server.Endpoint, connectCts));
            await connectWaiting.WaitAsync(GateTimeout);

            holdRelease.Set();
            await handler.Entered.Task.WaitAsync(GateTimeout);
            var budget = await budgets.WhenCreatedFor(connectCts.Token).WaitAsync(GateTimeout);
            connect.IsCompleted.Should().BeFalse("Connect() waits for the old Close while the handler runs");

            budget.Cancel();
            var connected = await connect.WaitAsync(GateTimeout);

            using (new AssertionScope())
            {
                connected.Should().BeTrue("running out of the bound does not fail Connect()");
                server.AcceptedConnections.Should().Be(2);
                _logs.HasWarning("is still running. Continuing without waiting").Should().BeTrue();
                stop.IsCompleted.Should().BeFalse("the Stop() that raised Close still waits for its handler");
            }

            handler.Finish();
            (await stop.WaitAsync(GateTimeout)).Should().BeTrue();
            (await client.Stop().WaitAsync(GateTimeout)).Should().BeTrue();
        }
        finally
        {
            holdRelease.Set();
            registration.Dispose();
        }
    }

    [Test]
    public void Close_Delivered_Should_Imply_Release_Completed()
    {
        var released = new AbstractWebSocketClient.ConnectionState(new System.Net.WebSockets.ClientWebSocket(), new CancellationTokenSource(), "released");
        released.CompleteRelease();

        var delivered = new AbstractWebSocketClient.ConnectionState(new System.Net.WebSockets.ClientWebSocket(), new CancellationTokenSource(), "delivered");
        delivered.CompleteCloseDelivery();

        using (new AssertionScope())
        {
            released.ReleaseCompleted.IsCompleted.Should().BeTrue();
            released.CloseDelivered.IsCompleted.Should().BeFalse("the release alone does not deliver Close");
            delivered.ReleaseCompleted.IsCompleted.Should().BeTrue("a delivered Close implies a finished release");
            delivered.CloseDelivered.IsCompleted.Should().BeTrue();
        }
    }

    // ---- Every v2 client: a server close wins, a user Stop() loses. ----

    private sealed class ListenClient(string apiKey, DeepgramWsClientOptions options)
        : Deepgram.Clients.Listen.v2.WebSocket.Client(apiKey, options), IExposesConnection
    {
        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class SpeakClient(string apiKey, DeepgramWsClientOptions options)
        : Deepgram.Clients.Speak.v2.WebSocket.Client(apiKey, options), IExposesConnection
    {
        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class AgentClient(string apiKey, DeepgramWsClientOptions options)
        : Deepgram.Clients.Agent.v2.WebSocket.Client(apiKey, options), IExposesConnection
    {
        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class FluxClient(string apiKey, DeepgramWsClientOptions options)
        : Deepgram.Clients.Flux.WebSocket.Client(apiKey, options), IExposesConnection
    {
        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    private sealed class FluxSpeakClient(string apiKey, DeepgramWsClientOptions options)
        : Deepgram.Clients.Flux.Speak.WebSocket.Client(apiKey, options), IExposesConnection
    {
        public CancellationToken CurrentToken => CurrentConnection!.Token;
    }

    /// <summary>Drives one kind of v2 WebSocket client.</summary>
    public sealed record ClientKind(
        string Name,
        Func<string, AbstractWebSocketClient> Create,
        Func<AbstractWebSocketClient, Task<bool>> Connect,
        Func<AbstractWebSocketClient, CancellationTokenSource?, Task<bool>> Stop,
        Func<AbstractWebSocketClient, Action, Task> SubscribeClose,
        string? ServerClosesOn = null)
    {
        public override string ToString() => Name;
    }

    private static readonly ClientKind[] Kinds =
    {
        new("Base",
            _ => new ScriptedClient(),
            c => c.Connect(((ScriptedClient)c).Endpoint!),
            (c, cts) => c.Stop(cts),
            (c, onClose) => c.Subscribe(new EventHandler<Common.CloseResponse>((_, _) => onClose()))),
        new("Listen",
            endpoint => { var key = NewApiKey(); return new ListenClient(key, Options(key, endpoint)); },
            c => ((ListenClient)c).Connect(new ListenModels.LiveSchema { Model = "nova-3" }),
            (c, cts) => ((ListenClient)c).Stop(cts),
            (c, onClose) => ((ListenClient)c).Subscribe(new EventHandler<ListenModels.CloseResponse>((_, _) => onClose()))),
        new("Speak",
            endpoint => { var key = NewApiKey(); return new SpeakClient(key, Options(key, endpoint)); },
            c => ((SpeakClient)c).Connect(new SpeakModels.SpeakSchema()),
            (c, cts) => ((SpeakClient)c).Stop(cts),
            (c, onClose) => ((SpeakClient)c).Subscribe(new EventHandler<SpeakModels.CloseResponse>((_, _) => onClose()))),
        new("Agent",
            endpoint => { var key = NewApiKey(); return new AgentClient(key, Options(key, endpoint)); },
            c => ((AgentClient)c).Connect(new AgentModels.SettingsSchema()),
            (c, cts) => ((AgentClient)c).Stop(cts),
            (c, onClose) => ((AgentClient)c).Subscribe(new EventHandler<AgentModels.CloseResponse>((_, _) => onClose()))),
        // The Flux STT/TTS servers close on CloseStream/Close, as the services do, so a final
        // Stop() does not wait out the close grace period.
        new("FluxSTT",
            endpoint => { var key = NewApiKey(); return new FluxClient(key, Options(key, endpoint)); },
            c => ((FluxClient)c).Connect(new FluxModels.FluxSchema { Model = "flux-general-en" }),
            (c, cts) => ((FluxClient)c).Stop(cts),
            (c, onClose) => ((FluxClient)c).Subscribe(new EventHandler<FluxModels.CloseResponse>((_, _) => onClose())),
            "CloseStream"),
        new("FluxTTS",
            endpoint => { var key = NewApiKey(); return new FluxSpeakClient(key, Options(key, endpoint)); },
            c => ((FluxSpeakClient)c).Connect(new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" }),
            (c, cts) => ((FluxSpeakClient)c).Stop(cts),
            (c, onClose) => ((FluxSpeakClient)c).Subscribe(new EventHandler<FluxSpeakModels.CloseResponse>((_, _) => onClose())),
            "Close"),
    };

    private static ClientKind KindNamed(string name) => Kinds.Single(k => k.Name == name);

    public static IEnumerable<TestCaseData> AllKinds() =>
        Kinds.Select(kind => new TestCaseData(kind.Name)
            .SetName($"Close_Should_Fire_Once_When_A_Losing_Stop_Is_Bounded_{kind.Name}"));

    /// <summary>
    /// Connection 1 closes from the server side when the test says so. Later connections close
    /// when the client sends the kind's close message, if it has one, as the services do.
    /// </summary>
    private static LoopbackWebSocketServer ServerFor(ClientKind kind, Task closeFirstConnection) => new(async connection =>
    {
        if (connection.Number == 1)
        {
            await closeFirstConnection;
            await connection.SendCloseAsync();
            return;
        }
        if (kind.ServerClosesOn is null)
        {
            return;
        }
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

    /// <summary>
    /// The server closes connection 1; its receive loop wins the release and raises Close, and
    /// the Close handler blocks. A user Stop() that snapshotted the connection before the release
    /// unpublished it is the losing teardown. Returns that Stop() once its bounded wait has started.
    /// </summary>
    private static async Task<(Task<bool> Loser, CancellationTokenSource Budget)> StartServerCloseRace(
        ClientKind kind, AbstractWebSocketClient client, CloseDeliveryBudgets budgets, BlockingCloseHandler handler,
        TaskCompletionSource<bool> closeNow, CancellationTokenSource? callerCts, CancellationToken loserToken)
    {
        // Hold the receive loop's release inside its first step, so the connection is still
        // published when the user Stop() snapshots it.
        var inRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var holdRelease = new ManualResetEventSlim(false);
        var connectionToken = ((IExposesConnection)client).CurrentToken;
        var registration = connectionToken.Register(() =>
        {
            inRelease.TrySetResult(true);
            holdRelease.Wait(GateTimeout);
        });
        try
        {
            closeNow.TrySetResult(true);
            await inRelease.Task.WaitAsync(GateTimeout);

            var loser = kind.Stop(client, callerCts);
            loser.IsCompleted.Should().BeFalse("Stop() never returns before its connection is released");
            budgets.WasCreatedFor(loserToken).Should().BeFalse("the wait for the release has no bound");

            holdRelease.Set();
            await handler.Entered.Task.WaitAsync(GateTimeout);
            var budget = await budgets.WhenCreatedFor(loserToken).WaitAsync(GateTimeout);
            loser.IsCompleted.Should().BeFalse("the loser waits for Close while the handler runs");
            return (loser, budget);
        }
        finally
        {
            holdRelease.Set();
            registration.Dispose();
            holdRelease.Dispose();
        }
    }

    [TestCaseSource(nameof(AllKinds))]
    public async Task Close_Should_Fire_Once_When_A_Losing_Stop_Is_Bounded(string kindName)
    {
        var kind = KindNamed(kindName);
        var closeNow = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = ServerFor(kind, closeNow.Task);
        using var client = kind.Create(server.Endpoint);
        if (client is ScriptedClient scripted)
        {
            scripted.Endpoint = server.Endpoint;
        }
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await kind.SubscribeClose(client, handler.Handle);
        (await kind.Connect(client).WaitAsync(GateTimeout)).Should().BeTrue();

        using var callerCts = new CancellationTokenSource();
        var (loser, budget) = await StartServerCloseRace(kind, client, budgets, handler, closeNow, callerCts, callerCts.Token);

        budget.Cancel();
        (await loser.WaitAsync(GateTimeout)).Should().BeTrue();
        handler.Events.Should().Be(1);

        handler.Finish();
        (await kind.Stop(client, null).WaitAsync(GateTimeout)).Should().BeTrue();

        // The client is not wedged: a new connection opens, and Close still fired once for the old one.
        (await kind.Connect(client).WaitAsync(GateTimeout)).Should().BeTrue();
        using (new AssertionScope())
        {
            server.AcceptedConnections.Should().Be(2);
            client.IsConnected().Should().BeTrue();
            handler.Events.Should().Be(1, "Close fires once for the stopped connection");
        }
        (await kind.Stop(client, null).WaitAsync(GateTimeout)).Should().BeTrue();
    }

    public static IEnumerable<TestCaseData> FluxKinds() =>
        new[] { "FluxSTT", "FluxTTS" }.Select(name => new TestCaseData(name)
            .SetName($"Flux_Stop_Without_A_Token_Should_Bound_Its_Wait_For_The_Receive_Loop_Close_{name}"));

    [TestCaseSource(nameof(FluxKinds))]
    public async Task Flux_Stop_Without_A_Token_Should_Bound_Its_Wait_For_The_Receive_Loop_Close(string kindName)
    {
        var kind = KindNamed(kindName);
        var closeNow = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = ServerFor(kind, closeNow.Task);
        using var client = kind.Create(server.Endpoint);
        var budgets = new CloseDeliveryBudgets();
        budgets.Attach(client);
        using var handler = new BlockingCloseHandler();
        await kind.SubscribeClose(client, handler.Handle);
        (await kind.Connect(client).WaitAsync(GateTimeout)).Should().BeTrue();

        // With no caller token, a Flux Stop() bounds its waits with the connection's own token.
        // The release has cancelled that token, so its wait for Close takes a fresh disconnect
        // timeout instead of skipping the wait.
        var connectionToken = ((IExposesConnection)client).CurrentToken;
        var (loser, budget) = await StartServerCloseRace(kind, client, budgets, handler, closeNow, null, connectionToken);

        budget.Cancel();
        var loserResult = await loser.WaitAsync(GateTimeout);
        using (new AssertionScope())
        {
            loserResult.Should().BeTrue();
            _logs.HasWarning("is still running. Continuing without waiting").Should().BeTrue();
            handler.Events.Should().Be(1);
        }

        handler.Finish();
        (await kind.Connect(client).WaitAsync(GateTimeout)).Should().BeTrue();
        server.AcceptedConnections.Should().Be(2);
        (await kind.Stop(client, null).WaitAsync(GateTimeout)).Should().BeTrue();
        handler.Events.Should().Be(2, "the second connection gets its own Close");
    }
}
