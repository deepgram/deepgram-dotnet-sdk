// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Deterministic regression tests for the #390 teardown race. Stop() and Dispose() both cancel and
/// dispose the shared internal CancellationTokenSource. A naive "if (field != null) { field.Cancel();
/// field.Dispose(); field = null; }" is a check-then-act race: two concurrent teardown calls (a
/// user Stop() overlapping the receive loop's server-close teardown, or Stop() racing Dispose()) can
/// throw NullReferenceException (field nulled between re-reads) or ObjectDisposedException. The fix
/// routes every teardown path through an Interlocked.Exchange claim so exactly one caller disposes.
///
/// These exercise the shared teardown directly and offline (no network), hammering the exact window
/// many times. Against the old check-then-act code this fails with NRE/ODE; against the fix it passes.
/// </summary>
public class WebSocketTeardownRaceTests
{
    // Minimal concrete client that exposes the protected teardown surface for testing. All members
    // used here are protected on AbstractWebSocketClient, so no reflection is needed.
    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient(string apiKey, IDeepgramClientOptions options) : base(apiKey, options) { }

        public void SeedTokenSource() => _cancellationTokenSource = new CancellationTokenSource();

        public CancellationTokenSource? CurrentTokenSource => _cancellationTokenSource;

        public void InvokeTeardown() => DisposeCancellationTokenSource();
    }

    private static TestWebSocketClient NewClient()
    {
        var apiKey = new Faker().Random.Guid().ToString();
        return new TestWebSocketClient(apiKey, new DeepgramWsClientOptions(apiKey) { OnPrem = true });
    }

    [Test]
    public async Task Concurrent_Teardown_Never_Throws_And_Claims_Once()
    {
        const int iterations = 300;
        const int concurrency = 8;

        for (var i = 0; i < iterations; i++)
        {
            var client = NewClient();
            client.SeedTokenSource();

            // Release all workers at once to maximize contention on the token-cleanup window.
            using var gate = new ManualResetEventSlim(false);
            var tasks = Enumerable.Range(0, concurrency).Select(_ => Task.Run(() =>
            {
                gate.Wait();
                client.InvokeTeardown();
            })).ToArray();

            gate.Set();

            Func<Task> act = () => Task.WhenAll(tasks);
            await act.Should().NotThrowAsync(
                "concurrent teardown must not race on the shared cancellation token source (#390)");

            client.CurrentTokenSource.Should().BeNull("the token source must be claimed and nulled exactly once");
        }
    }

    [Test]
    public void Repeated_Teardown_Is_Idempotent_And_Disposes_The_Source()
    {
        var client = NewClient();
        client.SeedTokenSource();
        var seeded = client.CurrentTokenSource!;

        using (new AssertionScope())
        {
            client.Invoking(c => c.InvokeTeardown()).Should().NotThrow();
            client.CurrentTokenSource.Should().BeNull("the first teardown claims and nulls the source");

            // The claimed source must actually be disposed, not just dropped.
            seeded.Invoking(s => _ = s.Token).Should().Throw<ObjectDisposedException>(
                "the claimed token source must be disposed");

            // Second and third teardowns are no-ops, never throwing on the now-null field.
            client.Invoking(c => c.InvokeTeardown()).Should().NotThrow();
            client.Invoking(c => c.InvokeTeardown()).Should().NotThrow();
        }
    }

    [Test]
    public void Teardown_With_No_Token_Source_Is_Safe()
    {
        var client = NewClient(); // never seeded; _cancellationTokenSource is null

        client.Invoking(c => c.InvokeTeardown()).Should().NotThrow(
            "teardown before any Connect() must be a safe no-op");
    }
}
