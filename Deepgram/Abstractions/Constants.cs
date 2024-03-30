// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Abstractions;

/// <summary> 
/// Defaults for the REST Client
/// </summary> 
public static class Constants
{
    // For Speak Headers
    public const int OneSecond = 1000;
    public const int OneMinute = 60 * OneSecond;
    public const int DefaultRESTTimeout = 5 * OneMinute;
}

