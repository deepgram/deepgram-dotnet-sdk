// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.TestExtensions;

public static class LoggerTestExtensions
{
    public static void AnyLogOfType<T>(this ILogger<T> logger, LogLevel level) where T : class
    {
        logger.Log(
            level,
            Arg.Any<EventId>(),
             Arg.Is<object>(o => o != null),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}