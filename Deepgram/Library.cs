// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram;

public class Library
{
    public static void Initialize(Logger.LogLevel level = Logger.LogLevel.Default, string? filename = "log.txt")
    {
        Log.Initialize(level, filename);
    }

    public static void Terminate()
    {
        // No-op
    }
}
