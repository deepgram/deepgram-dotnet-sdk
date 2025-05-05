// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Agent.v2.WebSocket;

public static class UriSegments
{
    // overriding the default uri segments which is typically api.deepgram.com
    // this is special for agent api for some odd reason
    public const string AGENT_URI = "agent.deepgram.com";

    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change
    public const string AGENT = "v1/agent/converse";
}
