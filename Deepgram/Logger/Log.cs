// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Serilog;
using Serilog.Events;

namespace Deepgram.Logger;

public sealed class Log
{
    private static Serilog.ILogger? instance = null;

    // Prevent instantiation
    static Log()
    {
        // Do nothing
    }
    
    /// <summary>
    /// Initializes the logger with the specified log level and filename.
    /// </summary>
    public static Serilog.ILogger Initialize(LogLevel level = LogLevel.Information, string? filename = "log.txt")
    {
        if (level == LogLevel.Disable)
        {
            instance = new LoggerConfiguration()
                .MinimumLevel.Is(((LogEventLevel)1 + (int)LogEventLevel.Fatal))
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            return instance;
        }
        if (filename != null)
        {
            instance = new LoggerConfiguration()
                .MinimumLevel.Is((LogEventLevel) level)
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File(filename, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            return instance;
        }
        instance = new LoggerConfiguration()
            .MinimumLevel.Is((LogEventLevel)level)
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        return instance;
    }

    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public static Serilog.ILogger GetLogger()
    {
        if (instance == null)
        {
            return Initialize();
        }
        return instance;
    }

    /// <summary>
    /// Logs a verbose message
    /// </summary>
    public static void Verbose(string identifier, string trace)
    {
        GetLogger().Verbose($"{identifier}: {trace}");
    }

    /// <summary>
    /// Logs a debug message
    /// </summary>
    public static void Debug(string identifier, string trace)
    {
        GetLogger().Debug($"{identifier}: {trace}");
    }

    /// <summary>
    /// Logs an information message
    /// </summary>
    public static void Information(string identifier, string trace)
    {
        GetLogger().Information($"{identifier}: {trace}");
    }

    /// <summary>
    /// Logs an warning message
    /// </summary>
    public static void Warning(string identifier, string trace)
    {
        GetLogger().Warning($"{identifier}: {trace}");
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    public static void Error(string identifier, string trace)
    {
        GetLogger().Error($"{identifier}: {trace}");
    }

    /// <summary>
    /// Logs an fatal message
    /// </summary>
    public static void Fatal(string identifier, string trace)
    {
        GetLogger().Fatal($"{identifier}: {trace}");
    }
}
