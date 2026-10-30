// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Microsoft.Extensions.Logging;
using ListenV1 = Deepgram.Clients.Listen.v1.WebSocket;
using ListenModels = Deepgram.Models.Listen.v1.WebSocket;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
using SpeakV1 = Deepgram.Clients.Speak.v1.WebSocket;
using SpeakModels = Deepgram.Models.Speak.v1.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

#pragma warning disable CS0618 // The v1 WebSocket clients are obsolete but still shipped; these tests cover them on purpose.

/// <summary>
/// The v1 Listen and Speak WebSocket clients must deliver every event to every subscribed handler,
/// even when one handler throws (#469).
///
/// Each test subscribes one throwing handler first, then many good handlers that each sleep a few
/// milliseconds. Before the fix, InvokeParallel's Parallel.ForEach stopped starting new handlers as
/// soon as the throwing one failed, and Connect() raised Open with a plain multicast Invoke that
/// stopped at the first throw, so some good handlers never ran. The sleeps keep the good handlers
/// busy long enough that the early stop is observed on every run, not by luck of scheduling.
/// </summary>
[NonParallelizable] // One test swaps the process-wide Log provider.
public class V1WebSocketHandlerIsolationTests
{
    private const int GoodHandlers = 64;
    private static readonly TimeSpan HandlerWork = TimeSpan.FromMilliseconds(5);

    private static string NewApiKey() => new Faker().Random.Guid().ToString("N");

    private static DeepgramWsClientOptions LoopbackOptions(string apiKey, int port) =>
        new(apiKey, $"ws://127.0.0.1:{port}", keepAlive: false, onPrem: true);

    private static MemoryStream TextFrame(string json) => new(Encoding.UTF8.GetBytes(json));

    private static void Deliver(ListenV1.Client client, string json)
    {
        using var ms = TextFrame(json);
        client.ProcessDataReceived(new WebSocketReceiveResult((int)ms.Length, WebSocketMessageType.Text, true), ms);
    }

    private static void Deliver(SpeakV1.Client client, MemoryStream ms, WebSocketMessageType type)
    {
        client.ProcessDataReceived(new WebSocketReceiveResult((int)ms.Length, type, true), ms);
    }

    /// <summary>
    /// Subscribes one throwing handler, then <see cref="GoodHandlers"/> good ones, through the
    /// supplied subscribe call. Returns a counter of good handler runs.
    /// </summary>
    private static StrongBox<int> SubscribeOneThrowingThenGood<T>(Func<EventHandler<T>, bool> subscribe)
    {
        var calls = new StrongBox<int>();
        subscribe((_, _) => throw new InvalidOperationException("Simulated handler failure"));
        for (var h = 0; h < GoodHandlers; h++)
        {
            subscribe((_, _) =>
            {
                Thread.Sleep(HandlerWork);
                Interlocked.Increment(ref calls.Value);
            });
        }
        return calls;
    }

    // --- InvokeParallel (every server message) ---

    [Test]
    public void ListenV1_Results_Should_Reach_Every_Handler_When_One_Throws()
    {
        using var client = new ListenV1.Client(NewApiKey());
        var calls = SubscribeOneThrowingThenGood<ListenModels.ResultResponse>(client.Subscribe);

        Deliver(client, "{\"type\":\"Results\"}");

        calls.Value.Should().Be(GoodHandlers, "a throwing Results handler must not stop the others");
    }

    [Test]
    public void ListenV1_Close_Message_Should_Reach_Every_Handler_When_One_Throws()
    {
        using var client = new ListenV1.Client(NewApiKey());
        var calls = SubscribeOneThrowingThenGood<ListenModels.CloseResponse>(client.Subscribe);

        Deliver(client, "{\"type\":\"Close\"}");

        calls.Value.Should().Be(GoodHandlers, "a throwing Close handler must not stop the others");
    }

    [Test]
    public void SpeakV1_Audio_Should_Reach_Every_Handler_When_One_Throws()
    {
        using var client = new SpeakV1.Client(NewApiKey());
        var calls = SubscribeOneThrowingThenGood<SpeakModels.AudioResponse>(client.Subscribe);

        using var audio = new MemoryStream(new byte[320]);
        Deliver(client, audio, WebSocketMessageType.Binary);

        calls.Value.Should().Be(GoodHandlers, "a throwing Audio handler must not stop the others");
    }

    [Test]
    public void SpeakV1_Close_Message_Should_Reach_Every_Handler_When_One_Throws()
    {
        using var client = new SpeakV1.Client(NewApiKey());
        var calls = SubscribeOneThrowingThenGood<SpeakModels.CloseResponse>(client.Subscribe);

        using var frame = TextFrame("{\"type\":\"Close\"}");
        Deliver(client, frame, WebSocketMessageType.Text);

        calls.Value.Should().Be(GoodHandlers, "a throwing Close handler must not stop the others");
    }

    /// <summary>
    /// Log providers are user code too. A provider that throws when the SDK logs a handler's
    /// exception must not stop delivery to the remaining handlers.
    /// </summary>
    [Test]
    public void ListenV1_Results_Should_Reach_Every_Handler_When_The_Log_Provider_Throws()
    {
        var provider = InstallThrowingLoggerProvider();
        try
        {
            using var client = new ListenV1.Client(NewApiKey());
            var calls = SubscribeOneThrowingThenGood<ListenModels.ResultResponse>(client.Subscribe);
            var errorsBefore = provider.ErrorCalls;

            Deliver(client, "{\"type\":\"Results\"}");

            (provider.ErrorCalls - errorsBefore).Should().BeGreaterThan(0,
                "the handler's exception must be logged through the throwing provider, or this test proves nothing");
            calls.Value.Should().Be(GoodHandlers, "a throwing log provider must not stop delivery");
        }
        finally
        {
            Log.Reset();
        }
    }

    /// <summary>
    /// Installs a <see cref="ThrowingLoggerProvider"/> as the SDK's log provider and returns it.
    /// Log.Configure is first-wins: if anything logs between Log.Reset() and Log.Configure() (for
    /// example a background loop left over from another test), the default provider is installed
    /// first and Configure is ignored. So retry until a probe message reaches our provider.
    /// </summary>
    /// <remarks>
    /// Once #456 lands, Log.ResetAndConfigureForTests can replace this loop.
    /// </remarks>
    private static ThrowingLoggerProvider InstallThrowingLoggerProvider()
    {
        const int maxAttempts = 50;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var provider = new ThrowingLoggerProvider();
            Log.Reset();
            Log.Configure(LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(MelLogLevel.Trace);
                builder.AddProvider(provider);
            }));

            var probe = $"install-probe-{Guid.NewGuid():N}";
            Log.Warning("V1WebSocketHandlerIsolationTests", probe);
            if (provider.HasSeen(probe))
            {
                return provider;
            }
        }

        Log.Reset();
        throw new InvalidOperationException($"Could not install the throwing log provider in {maxAttempts} attempts.");
    }

    // --- Open, raised by Connect() ---

    [Test]
    public async Task ListenV1_Connect_Should_Deliver_Open_To_Every_Handler_And_Stay_Connected_When_One_Throws()
    {
        using var server = new LoopbackWebSocketHandshakeServer();
        var apiKey = NewApiKey();
        var client = new ListenV1.Client(apiKey, LoopbackOptions(apiKey, server.Port));
        try
        {
            var calls = SubscribeOneThrowingThenGood<ListenModels.OpenResponse>(client.Subscribe);

            Func<Task> connect = () => client.Connect(new ListenModels.LiveSchema { Model = "nova-3" });

            await connect.Should().NotThrowAsync("a throwing Open handler must not fail Connect()");
            calls.Value.Should().Be(GoodHandlers, "a throwing Open handler must not stop the others");
            client.IsConnected().Should().BeTrue();
        }
        finally
        {
            client.Dispose();
        }
    }

    [Test]
    public async Task SpeakV1_Connect_Should_Deliver_Open_To_Every_Handler_And_Stay_Connected_When_One_Throws()
    {
        using var server = new LoopbackWebSocketHandshakeServer();
        var apiKey = NewApiKey();
        var client = new SpeakV1.Client(apiKey, LoopbackOptions(apiKey, server.Port));
        try
        {
            var calls = SubscribeOneThrowingThenGood<SpeakModels.OpenResponse>(client.Subscribe);

            Func<Task> connect = () => client.Connect(new SpeakModels.SpeakSchema());

            await connect.Should().NotThrowAsync("a throwing Open handler must not fail Connect()");
            calls.Value.Should().Be(GoodHandlers, "a throwing Open handler must not stop the others");
            client.IsConnected().Should().BeTrue();
        }
        finally
        {
            client.Dispose();
        }
    }

    // --- Close, raised by Stop() ---

    /// <summary>
    /// Records when each Close handler starts and ends. Close is raised once per connection, one
    /// handler at a time in subscription order: the second good handler must start only after the
    /// first one, which takes 50 ms, has finished.
    /// </summary>
    /// <remarks>
    /// On a 1-CPU machine the thread pool starts with one worker, so a parallel delivery could still
    /// happen to run the handlers one after another and pass. <see cref="WithSpareThreadPoolThreads"/>
    /// makes idle workers available, so a parallel delivery really does overlap the handlers.
    /// </remarks>
    private sealed class CloseRecorder
    {
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        public long FirstEnd = -1;
        public long SecondStart = -1;
        public int Calls;

        public void First()
        {
            Thread.Sleep(50);
            Interlocked.Exchange(ref FirstEnd, _clock.ElapsedTicks);
            Interlocked.Increment(ref Calls);
        }

        public void Second()
        {
            Interlocked.Exchange(ref SecondStart, _clock.ElapsedTicks);
            Interlocked.Increment(ref Calls);
        }

        public void Rest()
        {
            Thread.Sleep(HandlerWork);
            Interlocked.Increment(ref Calls);
        }

        public void AssertAllRanInOrder()
        {
            Calls.Should().Be(GoodHandlers, "a throwing Close handler must not stop the others");
            FirstEnd.Should().BeGreaterThan(-1);
            SecondStart.Should().BeGreaterThan(-1);
            SecondStart.Should().BeGreaterOrEqualTo(FirstEnd,
                "Close handlers run one at a time in subscription order, so handler 2 starts after handler 1 ends");
        }
    }

    private static CloseRecorder SubscribeClose<T>(Func<EventHandler<T>, bool> subscribe)
    {
        var recorder = new CloseRecorder();
        subscribe((_, _) => throw new InvalidOperationException("Simulated Close handler failure"));
        subscribe((_, _) => recorder.First());
        subscribe((_, _) => recorder.Second());
        for (var h = 2; h < GoodHandlers; h++)
        {
            subscribe((_, _) => recorder.Rest());
        }
        return recorder;
    }

    private static async Task WithSpareThreadPoolThreads(Func<Task> body)
    {
        ThreadPool.GetMinThreads(out var workers, out var io);
        ThreadPool.SetMinThreads(Math.Max(workers, 16), io).Should().BeTrue();
        try
        {
            await body();
        }
        finally
        {
            ThreadPool.SetMinThreads(workers, io);
        }
    }

    [Test]
    public async Task ListenV1_Stop_Should_Deliver_Close_To_Every_Handler_In_Order_When_One_Throws()
    {
        using var server = new LoopbackWebSocketHandshakeServer();
        var apiKey = NewApiKey();
        var client = new ListenV1.Client(apiKey, LoopbackOptions(apiKey, server.Port));
        try
        {
            var recorder = SubscribeClose<ListenModels.CloseResponse>(client.Subscribe);
            await client.Connect(new ListenModels.LiveSchema { Model = "nova-3" });
            client.IsConnected().Should().BeTrue();

            await WithSpareThreadPoolThreads(() => client.Stop());

            recorder.AssertAllRanInOrder();
        }
        finally
        {
            client.Dispose();
        }
    }

    [Test]
    public async Task SpeakV1_Stop_Should_Deliver_Close_To_Every_Handler_In_Order_When_One_Throws()
    {
        using var server = new LoopbackWebSocketHandshakeServer();
        var apiKey = NewApiKey();
        var client = new SpeakV1.Client(apiKey, LoopbackOptions(apiKey, server.Port));
        try
        {
            var recorder = SubscribeClose<SpeakModels.CloseResponse>(client.Subscribe);
            await client.Connect(new SpeakModels.SpeakSchema());
            client.IsConnected().Should().BeTrue();

            await WithSpareThreadPoolThreads(() => client.Stop());

            recorder.AssertAllRanInOrder();
        }
        finally
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// Accepts WebSocket upgrades on a loopback port, then reads and discards frames until the
    /// client goes away. Enough for Connect() to succeed and Stop() to send its Close frame.
    /// </summary>
    private sealed class LoopbackWebSocketHandshakeServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cts = new();
        private readonly List<TcpClient> _clients = new();

        public int Port { get; }

        public LoopbackWebSocketHandshakeServer()
        {
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(AcceptLoopAsync);
        }

        private async Task AcceptLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_cts.Token);
                }
                catch (Exception) when (_cts.IsCancellationRequested)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                lock (_clients)
                {
                    _clients.Add(client);
                }
                _ = Task.Run(() => ServeAsync(client));
            }
        }

        private async Task ServeAsync(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var request = new List<byte>();
                var one = new byte[1];
                while (true)
                {
                    if (await stream.ReadAsync(one, _cts.Token) == 0)
                    {
                        return;
                    }
                    request.Add(one[0]);
                    var n = request.Count;
                    if (n >= 4 && request[n - 4] == '\r' && request[n - 3] == '\n' && request[n - 2] == '\r' && request[n - 1] == '\n')
                    {
                        break;
                    }
                }

                var keyLine = Encoding.ASCII.GetString(request.ToArray())
                    .Split("\r\n")
                    .First(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));
                var key = keyLine.Substring(keyLine.IndexOf(':') + 1).Trim();
                var accept = Convert.ToBase64String(
                    SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                var response = Encoding.ASCII.GetBytes(
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Connection: Upgrade\r\n" +
                    $"Sec-WebSocket-Accept: {accept}\r\n\r\n");
                await stream.WriteAsync(response, _cts.Token);

                var sink = new byte[4096];
                while (await stream.ReadAsync(sink, _cts.Token) > 0)
                {
                }
            }
            catch (Exception) when (_cts.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    client.Dispose();
                }
            }
            _cts.Dispose();
        }
    }

    /// <summary>
    /// A log provider that throws on Error and above, and counts those calls. Lower levels are
    /// recorded so the installer can confirm the provider is live.
    /// </summary>
    private sealed class ThrowingLoggerProvider : ILoggerProvider
    {
        private int _errorCalls;
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> _messages = new();

        public int ErrorCalls => Volatile.Read(ref _errorCalls);

        public bool HasSeen(string text) => _messages.Any(m => m.Contains(text, StringComparison.Ordinal));

        public ILogger CreateLogger(string categoryName) => new ThrowingLogger(this);

        public void Dispose() { }

        private sealed class ThrowingLogger(ThrowingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel >= MelLogLevel.Error)
                {
                    Interlocked.Increment(ref provider._errorCalls);
                    throw new InvalidOperationException("Simulated log provider failure");
                }

                provider._messages.Enqueue(formatter(state, exception));
            }
        }
    }
}
