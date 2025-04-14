// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Auth.v1;

public static class UriSegments
{
    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change
    public const string GRANTTOKEN = "grant";
}
