// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.PreRecorded.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the UriSegments implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.REST` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.REST instead", false)]
public static class UriSegments
{
    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change

    public const string LISTEN = "listen";
}
