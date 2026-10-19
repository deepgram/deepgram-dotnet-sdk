// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenWebSocketAutoFlushTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private const string InterimResults =
        """{"type":"Results","channel_index":[0,1],"duration":1.0,"start":0.0,"is_final":false,"speech_final":false,"channel":{"alternatives":[{"transcript":"hello","confidence":0.9,"words":[]}]}}""";

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

        try
        {
            caller.Start();

            // ProcessTextMessage blocks in InspectMessage(...).Wait() after WaitAsync has queued
            // its waiter. Release once the caller has stayed parked; CallerProbe's remarks give
            // the residual risk.
            CallerProbe.WaitUntilReturnedOrParked(caller, processed.Task, WaitTimeout)
                .Should().BeTrue("the caller should reach the lock");
            mutex.Release();
            lockHeld = false;

            processed.Task.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue(
                "ProcessTextMessage must return once the lock is free, even when the caller's context never pumps");
            lockHeld = mutex.Wait(TimeSpan.FromSeconds(2));
            lockHeld.Should().BeTrue("InspectMessage must release _mutexLastDatagram");
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
