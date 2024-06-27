// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Speak.v1;

/// <summary>
// *********** WARNING ***********
// This package provides UriSegments for the Speak Client for the Deepgram API
//
// Deprecated: This class is deprecated. Use the namespace `Deepgram.Clients.Speak.v1.REST` instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Speak.v1.REST.UriSegments instead", false)]
public static class UriSegments
{
    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change
    public const string SPEAK = "speak";
}
