// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Logger;
using Microsoft.Extensions.Logging;
using DeepgramLogLevel = Deepgram.Logger.LogLevel;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.LoggerTests;

public class LogTests
{
    [SetUp]
    public void SetUp() => Log.Reset();

    [TearDown]
    public void TearDown() => Log.Reset();

    [Test]
    public void LogLevel_Should_Map_OneToOne_Onto_Microsoft_Extensions_Logging()
    {
        using (new AssertionScope())
        {
            ((int)DeepgramLogLevel.Verbose).Should().Be((int)MelLogLevel.Trace);
            ((int)DeepgramLogLevel.Debug).Should().Be((int)MelLogLevel.Debug);
            ((int)DeepgramLogLevel.Information).Should().Be((int)MelLogLevel.Information);
            ((int)DeepgramLogLevel.Default).Should().Be((int)MelLogLevel.Information);
            ((int)DeepgramLogLevel.Warning).Should().Be((int)MelLogLevel.Warning);
            ((int)DeepgramLogLevel.Error).Should().Be((int)MelLogLevel.Error);
            ((int)DeepgramLogLevel.Fatal).Should().Be((int)MelLogLevel.Critical);
            ((int)DeepgramLogLevel.Disable).Should().Be((int)MelLogLevel.None);
        }
    }

    [Test]
    public void Configure_Should_Throw_When_Factory_Is_Null()
    {
        Action act = () => Log.Configure(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Log_Should_Route_Message_To_Configured_Factory_Under_The_Identifier_Category()
    {
        // Arrange
        var provider = new RecordingLoggerProvider();
        Log.Configure(BuildFactory(provider, MelLogLevel.Trace));

        // Act
        Log.Information("ListenWSClient", "hello world");

        // Assert
        var entry = provider.Entries.Should().ContainSingle(e => e.Message == "hello world").Subject;
        entry.Category.Should().Be("ListenWSClient");
        entry.Level.Should().Be(MelLogLevel.Information);
    }

    [Test]
    public void Log_Should_Use_A_Distinct_Category_Per_Identifier()
    {
        var provider = new RecordingLoggerProvider();
        Log.Configure(BuildFactory(provider, MelLogLevel.Trace));

        Log.Debug("ComponentA", "a");
        Log.Debug("ComponentB", "b");

        // Filter out the "Logger initialized." entry Configure emits under the default category.
        provider.Entries.Where(e => e.Category != Log.DefaultCategory)
            .Select(e => e.Category)
            .Should().BeEquivalentTo(new[] { "ComponentA", "ComponentB" });
    }

    [Test]
    public void Log_Should_Respect_The_Factory_Minimum_Level()
    {
        var provider = new RecordingLoggerProvider();
        Log.Configure(BuildFactory(provider, MelLogLevel.Warning));

        Log.Information("X", "should be filtered");
        Log.Error("X", "should pass");

        provider.Entries.Should().ContainSingle().Which.Message.Should().Be("should pass");
    }

    [Test]
    public void IsEnabled_Should_Return_False_For_Disable()
    {
        Log.Configure(BuildFactory(new RecordingLoggerProvider(), MelLogLevel.Trace));
        Log.IsEnabled(DeepgramLogLevel.Disable).Should().BeFalse();
    }

    [Test]
    public void IsEnabled_Should_Reflect_The_Factory_Minimum_Level()
    {
        Log.Configure(BuildFactory(new RecordingLoggerProvider(), MelLogLevel.Warning));

        using (new AssertionScope())
        {
            Log.IsEnabled(DeepgramLogLevel.Information).Should().BeFalse();
            Log.IsEnabled(DeepgramLogLevel.Warning).Should().BeTrue();
            Log.IsEnabled(DeepgramLogLevel.Error).Should().BeTrue();
        }
    }

    [Test]
    public void Configure_Should_Be_First_Wins()
    {
        var first = new RecordingLoggerProvider();
        var second = new RecordingLoggerProvider();
        Log.Configure(BuildFactory(first, MelLogLevel.Trace));
        Log.Configure(BuildFactory(second, MelLogLevel.Trace));

        Log.Information("X", "routed to first");

        first.Entries.Should().ContainSingle(e => e.Message == "routed to first");
        second.Entries.Should().BeEmpty();
    }

    [Test]
    public void BeginScope_Should_Attach_State_To_Entries_Written_Within_It()
    {
        var provider = new RecordingLoggerProvider();
        Log.Configure(BuildFactory(provider, MelLogLevel.Trace));

        using (Log.BeginScope("ListenWSClient",
            new Dictionary<string, object> { ["dg.connection_id"] = "abc123" }))
        {
            Log.Information("ListenWSClient", "inside scope");
        }

        var entry = provider.Entries.Should().ContainSingle(e => e.Message == "inside scope").Subject;
        entry.Scopes.Should().ContainSingle(s =>
            s is IEnumerable<KeyValuePair<string, object>>);
    }

    private static ILoggerFactory BuildFactory(ILoggerProvider provider, MelLogLevel minimum) =>
        LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(minimum);
            builder.AddProvider(provider);
        });

    private sealed record RecordedEntry(MelLogLevel Level, string Category, string Message, IReadOnlyList<object?> Scopes);

    private sealed class RecordingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider? _scopeProvider;
        public ConcurrentQueue<RecordedEntry> Entries { get; } = new();

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, this);

        public void Dispose() { }

        private sealed class RecordingLogger : ILogger
        {
            private readonly string _category;
            private readonly RecordingLoggerProvider _provider;

            public RecordingLogger(string category, RecordingLoggerProvider provider)
            {
                _category = category;
                _provider = provider;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
                _provider._scopeProvider?.Push(state);

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var scopes = new List<object?>();
                _provider._scopeProvider?.ForEachScope((scope, list) => list.Add(scope), scopes);
                _provider.Entries.Enqueue(new RecordedEntry(logLevel, _category, formatter(state, exception), scopes));
            }
        }
    }
}
