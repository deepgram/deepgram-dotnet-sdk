// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Deepgram.Tests.Fakes;

/// <summary>
/// Tells a test when a caller thread has reached a lock the test holds, so the test can
/// release the lock at the right moment.
/// </summary>
public static class CallerProbe
{
    /// <summary>
    /// How long the caller must stay parked before it counts as blocked on the lock.
    /// </summary>
    public static readonly TimeSpan StableParkWindow = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Returns true once <paramref name="returned"/> has completed, or once
    /// <paramref name="caller"/> has stayed in <see cref="System.Threading.ThreadState.WaitSleepJoin"/>
    /// for <see cref="StableParkWindow"/> on every poll. Returns false at <paramref name="timeout"/>.
    /// </summary>
    /// <remarks>
    /// SemaphoreSlim exposes no waiter count, so thread state is the only external signal for a
    /// caller blocked in a synchronous wait. A brief WaitSleepJoin before the caller reaches the
    /// lock (for example a contended lock inside logging) resets as soon as the thread runs
    /// again, so it does not count. The residual risk is a caller that is parked somewhere else
    /// for the whole window before it reaches the lock; tests built on this probe only pass
    /// falsely in that case, they never fail falsely.
    /// </remarks>
    public static bool WaitUntilReturnedOrParked(Thread caller, Task returned, TimeSpan timeout)
    {
        var total = Stopwatch.StartNew();
        Stopwatch? parked = null;
        var spinner = new SpinWait();

        while (total.Elapsed < timeout)
        {
            if (returned.IsCompleted)
            {
                return true;
            }

            if ((caller.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0)
            {
                parked ??= Stopwatch.StartNew();
                if (parked.Elapsed >= StableParkWindow)
                {
                    return true;
                }
            }
            else
            {
                parked = null;
            }

            spinner.SpinOnce();
        }

        return false;
    }
}
