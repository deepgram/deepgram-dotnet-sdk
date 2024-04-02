// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Logger;

/// <summary>
/// Specifies the meaning and relative importance of a log event.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Anything and everything you might want to know about
    /// a running block of code.
    /// </summary>
    Verbose = Serilog.Events.LogEventLevel.Verbose,

    /// <summary>
    /// Internal system events that aren't necessarily
    /// observable from the outside.
    /// </summary>
    Debug = Serilog.Events.LogEventLevel.Debug,

    /// <summary>
    /// The lifeblood of operational intelligence - things
    /// happen.
    /// </summary>
    Information = Serilog.Events.LogEventLevel.Information,
    Default = Serilog.Events.LogEventLevel.Information,

    /// <summary>
    /// Service is degraded or endangered.
    /// </summary>
    Warning = Serilog.Events.LogEventLevel.Warning,

    /// <summary>
    /// Functionality is unavailable, invariants are broken
    /// or data is lost.
    /// </summary>
    Error = Serilog.Events.LogEventLevel.Error,

    /// <summary>
    /// If you have a pager, it goes off when one of these
    /// occurs.
    /// </summary>
    Fatal = Serilog.Events.LogEventLevel.Fatal,
}
