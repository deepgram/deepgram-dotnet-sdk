// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Logger;

/// <summary>
/// A minimal <see cref="ILoggerProvider"/> that appends log entries to a file.
/// <para>
/// Microsoft.Extensions.Logging does not ship a built-in file sink, so this provider
/// preserves the file-logging behavior that <see cref="Deepgram.Library.Initialize"/>
/// offered when the SDK used Serilog. Consumers who want richer file logging should
/// supply their own <see cref="ILoggerFactory"/> via <see cref="Log.Configure(ILoggerFactory)"/>
/// and add the file provider of their choice.
/// </para>
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly object _sync = new();
    private bool _disposed;

    public FileLoggerProvider(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A file path is required.", nameof(filePath));
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        {
            AutoFlush = true,
        };
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

    internal void Write(string line)
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _writer.Write(line);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _writer.Dispose();
        }
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string category, FileLoggerProvider provider)
        {
            _category = category;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

        public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.Append(" [").Append(LevelLabel(logLevel)).Append("] ");
            builder.Append(_category).Append(": ").Append(message);
            builder.Append(Environment.NewLine);
            if (exception is not null)
            {
                builder.Append(exception).Append(Environment.NewLine);
            }

            _provider.Write(builder.ToString());
        }

        private static string LevelLabel(MelLogLevel level) => level switch
        {
            MelLogLevel.Trace => "Verbose",
            MelLogLevel.Debug => "Debug",
            MelLogLevel.Information => "Information",
            MelLogLevel.Warning => "Warning",
            MelLogLevel.Error => "Error",
            MelLogLevel.Critical => "Fatal",
            _ => level.ToString(),
        };
    }
}
