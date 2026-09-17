// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.LoggerTests;

public class FileLoggerProviderTests
{
    private string _path = null!;

    [SetUp]
    public void SetUp() => _path = Path.Combine(Path.GetTempPath(), $"dg-filelogger-{Guid.NewGuid():N}.log");

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Test]
    public void Provider_Should_Append_Entries_With_Level_Category_And_Message()
    {
        using (var provider = new FileLoggerProvider(_path))
        {
            var logger = provider.CreateLogger("ManageClient");
            logger.Log(MelLogLevel.Information, new EventId(0), "the message", null, (s, _) => s);
        }

        var contents = File.ReadAllText(_path);
        using (new AssertionScope())
        {
            contents.Should().Contain("[Information]");
            contents.Should().Contain("ManageClient: the message");
        }
    }

    [Test]
    public void Provider_Should_Not_Write_When_Level_Is_None()
    {
        using (var provider = new FileLoggerProvider(_path))
        {
            var logger = provider.CreateLogger("X");
            logger.Log(MelLogLevel.None, new EventId(0), "suppressed", null, (s, _) => s);
        }

        File.ReadAllText(_path).Should().BeEmpty();
    }
}
