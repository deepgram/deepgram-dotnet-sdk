// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using AgentModels = Deepgram.Models.Agent.v2.WebSocket;
using FluxModels = Deepgram.Models.Flux.WebSocket;
using FluxSpeakModels = Deepgram.Models.Flux.Speak.WebSocket;
using ListenModels = Deepgram.Models.Listen.v2.WebSocket;
using SpeakModels = Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Blocking calls (<c>Stop().Wait()</c>, <c>Connect().Wait()</c>) on a thread whose
/// SynchronizationContext never runs posted callbacks, as on a UI thread that is blocked in the
/// call. Every await on these paths must use ConfigureAwait(false), or its continuation is posted
/// to that context and the call never returns.
/// </summary>
[NonParallelizable]
public class WebSocketSynchronizationContextTests
{
    // Upper bound for any single wait; reached only when a test is about to fail.
    private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

    private static string NewApiKey() => new Faker().Random.Guid().ToString();

    private static DeepgramWsClientOptions Options(string apiKey, string endpoint) =>
        new(apiKey, endpoint, keepAlive: false, onPrem: true);

    /// <summary>A UI-like context whose only thread is blocked: posted callbacks never run.</summary>
    private sealed class BlockedThreadSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
        }

        public override void Send(SendOrPostCallback d, object? state) =>
            throw new NotSupportedException("The blocked thread cannot run callbacks.");
    }

    /// <summary>Runs <paramref name="call"/> on a new thread that has the blocked context installed.</summary>
    private static bool RunOnBlockedThread(Func<bool> call)
    {
        var result = false;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new BlockedThreadSynchronizationContext());
            result = call();
        });
        thread.Start();
        thread.Join();
        return result;
    }

    /// <summary>A base client whose SendClose() sends nothing.</summary>
    private sealed class BaseClient : AbstractWebSocketClient
    {
        public BaseClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;
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
            endpoint => { var key = NewApiKey(); return new Deepgram.Clients.Listen.v2.WebSocket.Client(key, Options(key, endpoint)); },
            (c, _) => ((Deepgram.Clients.Listen.v2.WebSocket.Client)c).Connect(new ListenModels.LiveSchema { Model = "nova-3" }),
            c => ((Deepgram.Clients.Listen.v2.WebSocket.Client)c).Stop()),
        new("Speak",
            endpoint => { var key = NewApiKey(); return new Deepgram.Clients.Speak.v2.WebSocket.Client(key, Options(key, endpoint)); },
            (c, _) => ((Deepgram.Clients.Speak.v2.WebSocket.Client)c).Connect(new SpeakModels.SpeakSchema()),
            c => ((Deepgram.Clients.Speak.v2.WebSocket.Client)c).Stop()),
        new("Agent",
            endpoint => { var key = NewApiKey(); return new Deepgram.Clients.Agent.v2.WebSocket.Client(key, Options(key, endpoint)); },
            (c, _) => ((Deepgram.Clients.Agent.v2.WebSocket.Client)c).Connect(new AgentModels.SettingsSchema()),
            c => ((Deepgram.Clients.Agent.v2.WebSocket.Client)c).Stop()),
        // The Flux STT/TTS servers close on CloseStream/Close, as the services do, so Stop() does not
        // wait out its grace period.
        new("FluxSTT",
            endpoint => { var key = NewApiKey(); return new Deepgram.Clients.Flux.WebSocket.Client(key, Options(key, endpoint)); },
            (c, _) => ((Deepgram.Clients.Flux.WebSocket.Client)c).Connect(new FluxModels.FluxSchema { Model = "flux-general-en" }),
            c => ((Deepgram.Clients.Flux.WebSocket.Client)c).Stop(),
            "CloseStream"),
        new("FluxTTS",
            endpoint => { var key = NewApiKey(); return new Deepgram.Clients.Flux.Speak.WebSocket.Client(key, Options(key, endpoint)); },
            (c, _) => ((Deepgram.Clients.Flux.Speak.WebSocket.Client)c).Connect(new FluxSpeakModels.SpeakSchema { Model = "flux-alexis-en" }),
            c => ((Deepgram.Clients.Flux.Speak.WebSocket.Client)c).Stop(),
            "Close"),
    };

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

    public static IEnumerable<TestCaseData> StopKinds() =>
        Kinds.Select(kind => new TestCaseData(kind.Name)
            .SetName($"Stop_Wait_On_A_Blocking_SynchronizationContext_Should_Not_Deadlock_{kind.Name}"));

    public static IEnumerable<TestCaseData> ConnectKinds() =>
        Kinds.Select(kind => new TestCaseData(kind.Name)
            .SetName($"Connect_Wait_On_A_Blocking_SynchronizationContext_Should_Not_Deadlock_{kind.Name}"));

    [TestCaseSource(nameof(StopKinds))]
    public async Task Stop_Wait_On_A_Blocking_SynchronizationContext_Should_Not_Deadlock(string kindName)
    {
        var kind = Kinds.Single(k => k.Name == kindName);
        using var server = ServerFor(kind);
        using var client = kind.Create(server.Endpoint);
        (await kind.Connect(client, server.Endpoint).WaitAsync(GateTimeout)).Should().BeTrue();

        var returned = RunOnBlockedThread(() =>
        {
            var stop = kind.Stop(client);
            return stop.Wait(GateTimeout) && stop.Result;
        });

        returned.Should().BeTrue("Stop().Wait() on a blocked UI-like thread must not deadlock");
    }

    [TestCaseSource(nameof(ConnectKinds))]
    public async Task Connect_Wait_On_A_Blocking_SynchronizationContext_Should_Not_Deadlock(string kindName)
    {
        var kind = Kinds.Single(k => k.Name == kindName);
        using var server = ServerFor(kind);
        using var client = kind.Create(server.Endpoint);

        var returned = RunOnBlockedThread(() =>
        {
            var connect = kind.Connect(client, server.Endpoint);
            return connect.Wait(GateTimeout) && connect.Result;
        });

        returned.Should().BeTrue("Connect().Wait() on a blocked UI-like thread must not deadlock");
        await kind.Stop(client).WaitAsync(GateTimeout);
    }
}
