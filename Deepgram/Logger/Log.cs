// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Logger;

/// <summary>
/// The SDK's logging facade, built on <c>Microsoft.Extensions.Logging</c>.
/// <para>
/// By default the SDK logs to the console. To route SDK logs through your own logging
/// pipeline (Serilog, NLog, OpenTelemetry, Kibana/JSON, etc.) — without the SDK ever
/// reconfiguring your application's global logging — supply your own
/// <see cref="ILoggerFactory"/> via <see cref="Configure(ILoggerFactory)"/> (or register
/// it through dependency injection with <c>AddDeepgramLogging</c>).
/// </para>
/// <para>
/// Each SDK component logs under its own category (e.g. <c>"ListenWSClient"</c>), so
/// consumers can filter and format SDK logs per category with their own provider.
/// </para>
/// </summary>
public sealed class Log
{
    internal const string DefaultCategory = "Deepgram";

    private static readonly object _sync = new();
    private static readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private static ILoggerFactory? _factory;

    // True when the current factory is a built-in one the SDK created (and therefore owns and
    // may dispose). False when a consumer supplied their own factory via Configure — we never
    // dispose those.
    private static bool _ownsFactory;

    // Prevent instantiation
    static Log()
    {
        // Do nothing
    }

    /// <summary>
    /// Configures the SDK to emit its logs through the supplied <see cref="ILoggerFactory"/>.
    /// This is the recommended entry point for consumers who want the SDK to use their own
    /// logging pipeline. The SDK only consumes the factory; it never mutates the consumer's
    /// global/default logging configuration.
    /// </summary>
    /// <param name="factory">The logger factory the SDK should create its loggers from.</param>
    public static void Configure(ILoggerFactory factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        lock (_sync)
        {
            if (_factory != null)
            {
                GetLogger().LogWarning("Attempt to re-initialize logger ignored.");
                return;
            }

            SetFactory(factory, ownsFactory: false);
        }

        GetLogger().LogInformation("Logger initialized.");
    }

    /// <summary>
    /// Initializes the SDK's built-in logging with the specified log level and (optionally) a
    /// file to append logs to. Emits to the console, and — when <paramref name="filename"/> is
    /// non-null — also to that file.
    /// </summary>
    /// <remarks>
    /// This is a convenience for quick-start scenarios. Prefer <see cref="Configure(ILoggerFactory)"/>
    /// in applications that already own a logging pipeline.
    /// </remarks>
    public static ILogger Initialize(LogLevel level = LogLevel.Information, string? filename = "log.txt")
    {
        lock (_sync)
        {
            // Matches historical behavior: initializing the built-in logger (re)configures it
            // on every call rather than being ignored after the first.
            SetFactory(BuildDefaultFactory(level, filename), ownsFactory: true);
        }

        return GetLogger();
    }

    /// <summary>
    /// Returns whether a specific log level has been enabled.
    /// </summary>
    public static bool IsEnabled(LogLevel level)
    {
        if (level == LogLevel.Disable)
        {
            // There is no equivalent of Disable
            return false;
        }

        return GetLogger().IsEnabled(ToMel(level));
    }

    /// <summary>
    /// Gets the logger for the default SDK category. Configures the built-in console logger
    /// on first use if no <see cref="ILoggerFactory"/> has been supplied.
    /// </summary>
    public static ILogger GetLogger() => GetLogger(DefaultCategory);

    /// <summary>
    /// Gets the logger for the specified category, creating it (and the default factory, if
    /// needed) on first use.
    /// </summary>
    internal static ILogger GetLogger(string category)
    {
        var factory = GetFactory();
        return _loggers.GetOrAdd(category, c => factory.CreateLogger(c));
    }

    /// <summary>
    /// Begins a logical logging scope under the given category. All log entries written while
    /// the returned scope is alive carry the supplied state, which lets consumers correlate a
    /// group of related SDK log entries (for example, every entry for a single WebSocket
    /// connection) so they can later be tied back to a <c>request_id</c>.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that ends the scope when disposed.</returns>
    public static IDisposable? BeginScope(string identifier, IReadOnlyDictionary<string, object> state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        return GetLogger(identifier).BeginScope(state);
    }

    /// <summary>
    /// Logs a verbose message.
    /// </summary>
    public static void Verbose(string identifier, string trace) => Write(identifier, LogLevel.Verbose, trace);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public static void Debug(string identifier, string trace) => Write(identifier, LogLevel.Debug, trace);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    public static void Information(string identifier, string trace) => Write(identifier, LogLevel.Information, trace);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warning(string identifier, string trace) => Write(identifier, LogLevel.Warning, trace);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void Error(string identifier, string trace) => Write(identifier, LogLevel.Error, trace);

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    public static void Fatal(string identifier, string trace) => Write(identifier, LogLevel.Fatal, trace);

    private static void Write(string identifier, LogLevel level, string trace)
    {
        var logger = GetLogger(identifier);
        var melLevel = ToMel(level);
        if (!logger.IsEnabled(melLevel))
        {
            return;
        }

#pragma warning disable CA2254 // trace is a pre-formatted string, not a message template
        logger.Log(melLevel, "{Message}", trace);
#pragma warning restore CA2254
    }

    private static ILoggerFactory GetFactory()
    {
        if (_factory != null)
        {
            return _factory;
        }

        lock (_sync)
        {
            if (_factory == null)
            {
                SetFactory(BuildDefaultFactory(LogLevel.Information, null), ownsFactory: true);
            }
        }

        return _factory!;
    }

    private static void SetFactory(ILoggerFactory factory, bool ownsFactory)
    {
        // Dispose the previous factory only if we created it; never dispose a consumer's factory.
        if (_ownsFactory)
        {
            _factory?.Dispose();
        }

        _factory = factory;
        _ownsFactory = ownsFactory;
        _loggers.Clear();
    }

    /// <summary>
    /// Clears the configured factory and cached loggers. Test-only seam so each test can start
    /// from a clean logging state.
    /// </summary>
    internal static void Reset()
    {
        lock (_sync)
        {
            if (_ownsFactory)
            {
                _factory?.Dispose();
            }

            _factory = null;
            _ownsFactory = false;
            _loggers.Clear();
        }
    }

    private static ILoggerFactory BuildDefaultFactory(LogLevel level, string? filename)
    {
        var melLevel = ToMel(level);
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(melLevel);

            if (level != LogLevel.Disable)
            {
                builder.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                    options.SingleLine = true;
                    options.IncludeScopes = true;
                });

                if (filename != null)
                {
                    builder.AddProvider(new FileLoggerProvider(filename));
                }
            }
        });
    }

    private static MelLogLevel ToMel(LogLevel level) => (MelLogLevel)(int)level;
}
