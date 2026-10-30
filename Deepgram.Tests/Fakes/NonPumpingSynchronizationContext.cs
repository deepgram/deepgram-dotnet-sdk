// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Tests.Fakes;

/// <summary>
/// Models a single-threaded context (a UI thread) whose only thread is blocked: posted
/// callbacks never run.
/// </summary>
public sealed class NonPumpingSynchronizationContext : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? state)
    {
    }

    public override void Send(SendOrPostCallback d, object? state) =>
        throw new NotSupportedException("The blocked thread cannot run callbacks.");
}
