// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Abstractions.v2;

/// <summary> 
/// Defaults for the REST and WS AbstractClient
/// </summary> 
public static class Constants
{
    // For REST
    public const int OneSecond = 1000;
    public const int OneMinute = 60 * OneSecond;
    public const int DefaultRESTTimeout = 30 * OneSecond;

    // For WS
    public const int BufferSize = 1024 * 16;
    public const int UseArrayLengthForSend = -1;

    public const int DefaultConnectTimeout = 5000;
    public const int DefaultDisconnectTimeout = 5000;
}

