// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Net.WebSockets;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenWebSocketAutoFlushTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private const string InterimResults =
        """{"type":"Results","channel_index":[0,1],"duration":1.0,"start":0.0,"is_final":false,"speech_final":false,"channel":{"alternatives":[{"transcript":"hello","confidence":0.9,"words":[]}]}}""";

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
}
