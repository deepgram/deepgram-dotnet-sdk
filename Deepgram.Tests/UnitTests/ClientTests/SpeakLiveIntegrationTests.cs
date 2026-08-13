// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using FluentAssertions;
using FluentAssertions.Execution;
using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// End-to-end test against the real Speak (TTS) websocket endpoint, proving the #355 fix: text
/// containing quotation marks and backslashes must be sent as valid JSON so the server keeps the
/// socket open and returns audio. Before the fix, Regex.Unescape corrupted the payload and the
/// server closed the connection. Skipped unless DEEPGRAM_API_KEY is set, so CI never touches the network.
/// </summary>
public class SpeakLiveIntegrationTests
{
    private static string? ResolveApiKey()
    {
        return GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
    }

    [Test]
    public async Task Live_Speak_WebSocket_With_Quoted_Text_Stays_Open_And_Returns_Audio()
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Speak websocket test.");
        }

        var client = ClientFactory.CreateSpeakWebSocketClient(apiKey!);

        var audioBytes = 0L;
        var flushed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
        {
            if (e.Stream is not null)
            {
                Interlocked.Add(ref audioBytes, e.Stream.Length);
            }
        }));
        await client.Subscribe(new EventHandler<FlushedResponse>((_, _) => flushed.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));
        await client.Subscribe(new EventHandler<CloseResponse>((_, _) => closeReceived.TrySetResult(true)));

        var connected = await client.Connect(new SpeakSchema
        {
            Model = "aura-2-thalia-en",
            Encoding = "linear16",
            SampleRate = 24000,
        });
        connected.Should().BeTrue("the client must connect to the live Speak endpoint");

        // The crux of #355: quotes and backslashes in the text must not corrupt the wire payload.
        client.SpeakWithText("She said \"hello\" to the CEO & left. Path: C:\\temp\\notes.");
        client.Flush();

        var flushedInTime = await Task.WhenAny(flushed.Task, Task.Delay(30000)) == flushed.Task;

        await client.Stop();

        using (new AssertionScope())
        {
            flushedInTime.Should().BeTrue("the server must acknowledge the flushed text instead of closing the socket");
            audioBytes.Should().BeGreaterThan(0, "quoted/escaped text must still synthesize audio");
            errors.Should().BeEmpty("no error should be raised for text containing quotes or backslashes");
        }
    }

    [Test]
    public async Task Live_Speak_WebSocket_Concurrent_Stop_And_Dispose_Do_Not_Throw()
    {
        // End-to-end guard for #390: on a real connection, a user Stop() overlapping the receive
        // loop's server-close teardown (and an overlapping Dispose()) must not surface a
        // NullReferenceException/ObjectDisposedException from the shared cancellation token source.
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Speak teardown-race test.");
        }

        // Several connect/teardown cycles, each tearing down from multiple threads at once.
        for (var i = 0; i < 5; i++)
        {
            var client = ClientFactory.CreateSpeakWebSocketClient(apiKey!);

            var connected = await client.Connect(new SpeakSchema
            {
                Model = "aura-2-thalia-en",
                Encoding = "linear16",
                SampleRate = 24000,
            });
            connected.Should().BeTrue();

            client.SpeakWithText("Concurrent teardown test.");
            client.Flush();

            using var gate = new ManualResetEventSlim(false);
            var teardowns = new List<Task>
            {
                Task.Run(() => { gate.Wait(); return client.Stop(); }),
                Task.Run(() => { gate.Wait(); return client.Stop(); }),
                Task.Run(() => { gate.Wait(); ((IDisposable)client).Dispose(); }),
            };
            gate.Set();

            Func<Task> act = () => Task.WhenAll(teardowns);
            await act.Should().NotThrowAsync(
                $"iteration {i}: concurrent Stop()/Dispose() must not race on the token source (#390)");
        }
    }

    [Test]
    public async Task Live_Speak_WebSocket_Concurrent_Stop_NullByte_And_Dispose_Do_Not_Throw()
    {
        // End-to-end guard for the SendClose(nullByte:true) fast-path missed by the initial #390
        // hardening. Stop(nullByte:true) routes through SendClose(true), which sends a raw null byte
        // outside the base SendMessageImmediately helper; before the fix it dereferenced
        // _clientWebSocket and _cancellationTokenSource.Token AFTER awaiting _mutexSend, so a
        // concurrent Dispose() nulling those fields threw NullReferenceException. An NRE is not in
        // Stop()'s benign catch set, so it escaped Stop(); the fix makes SendClose teardown-safe so
        // any surviving exception is a benign cancellation that Stop() already absorbs.
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Speak null-byte teardown-race test.");
        }

        for (var i = 0; i < 5; i++)
        {
            var client = ClientFactory.CreateSpeakWebSocketClient(apiKey!);

            var connected = await client.Connect(new SpeakSchema
            {
                Model = "aura-2-thalia-en",
                Encoding = "linear16",
                SampleRate = 24000,
            });
            connected.Should().BeTrue();

            client.SpeakWithText("Concurrent null-byte close test.");
            client.Flush();

            using var gate = new ManualResetEventSlim(false);
            var teardowns = new List<Task>
            {
                Task.Run(() => { gate.Wait(); return client.Stop(nullByte: true); }),
                Task.Run(() => { gate.Wait(); return client.Stop(nullByte: true); }),
                Task.Run(() => { gate.Wait(); ((IDisposable)client).Dispose(); }),
            };
            gate.Set();

            Func<Task> act = () => Task.WhenAll(teardowns);
            await act.Should().NotThrowAsync(
                $"iteration {i}: concurrent Stop(nullByte:true)/Dispose() must not NRE on the null-byte fast-path (#390)");
        }
    }
}
