// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Logger;

public sealed class LogProvider
{
    internal static readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    internal static ILoggerFactory? _loggerFactory = NullLoggerFactory.Instance;

    public static void SetLogFactory(ILoggerFactory factory)
    {
        _loggerFactory?.Dispose();
        _loggerFactory = factory;
        _loggers.Clear();
    }

    public static ILogger GetLogger(string category)
    {
        // Try to get the logger from the dictionary
        if (!_loggers.TryGetValue(category, out var logger))
        {
            // If not found, create a new logger and add it to the dictionary
            logger = _loggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
            _loggers[category] = logger;
        }
        // Return the logger
        return logger;
    }
}
