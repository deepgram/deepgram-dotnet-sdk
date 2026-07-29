// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;

namespace Deepgram;

public class Library
{
    /// <summary>
    /// Initializes the SDK's built-in logging (console, plus an optional file). Prefer
    /// <see cref="Configure(ILoggerFactory)"/> in applications that already own a logging pipeline.
    /// </summary>
    public static void Initialize(Logger.LogLevel level = Logger.LogLevel.Default, string? filename = "log.txt")
    {
        Log.Initialize(level, filename);
    }

    /// <summary>
    /// Routes SDK logs through the supplied <see cref="ILoggerFactory"/> instead of the built-in
    /// console logger. The SDK never reconfigures the consumer's global logging.
    /// </summary>
    public static void Configure(ILoggerFactory factory)
    {
        Log.Configure(factory);
    }

    public static void Terminate()
    {
        // No-op
    }
}
