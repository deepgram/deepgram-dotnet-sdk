// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Manage.v1;

public static class UriSegments
{
    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change
    public const string PROJECTS = "projects";
    public const string BALANCES = "balances";
    public const string USAGE = "usage";
    public const string MEMBERS = "members";
    public const string KEYS = "keys";
    public const string INVITES = "invites";
    public const string SCOPES = "scopes";
    public const string REQUESTS = "requests";
    public const string LISTEN = "listen";
}
