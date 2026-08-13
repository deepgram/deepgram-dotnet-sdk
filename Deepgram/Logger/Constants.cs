// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Logger;

/// <summary>
/// Specifies the meaning and relative importance of a log event.
/// </summary>
/// <remarks>
/// The values map one-to-one onto <see cref="Microsoft.Extensions.Logging.LogLevel"/>,
/// which the SDK now uses under the hood. The member names are preserved from earlier
/// releases so existing code that references <c>Deepgram.Logger.LogLevel</c> keeps compiling.
/// </remarks>
public enum LogLevel
{
    /// <summary>
    /// Anything and everything you might want to know about
    /// a running block of code. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Trace"/>.
    /// </summary>
    Verbose = Microsoft.Extensions.Logging.LogLevel.Trace,

    /// <summary>
    /// Internal system events that aren't necessarily
    /// observable from the outside. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Debug"/>.
    /// </summary>
    Debug = Microsoft.Extensions.Logging.LogLevel.Debug,

    /// <summary>
    /// The lifeblood of operational intelligence - things
    /// happen. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>.
    /// </summary>
    Information = Microsoft.Extensions.Logging.LogLevel.Information,
    Default = Microsoft.Extensions.Logging.LogLevel.Information,

    /// <summary>
    /// Service is degraded or endangered. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/>.
    /// </summary>
    Warning = Microsoft.Extensions.Logging.LogLevel.Warning,

    /// <summary>
    /// Functionality is unavailable, invariants are broken
    /// or data is lost. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>.
    /// </summary>
    Error = Microsoft.Extensions.Logging.LogLevel.Error,

    /// <summary>
    /// If you have a pager, it goes off when one of these
    /// occurs. Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.Critical"/>.
    /// </summary>
    Fatal = Microsoft.Extensions.Logging.LogLevel.Critical,

    /// <summary>
    /// Disable logging... Do so at your own peril.
    /// Maps to <see cref="Microsoft.Extensions.Logging.LogLevel.None"/>.
    /// </summary>
    Disable = Microsoft.Extensions.Logging.LogLevel.None
}
