// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Flux.WebSocket;

/// <summary>
/// (PREVIEW) URI segments for the Flux (v2 listen) WebSocket client.
/// </summary>
public static class UriSegments
{
    // Flux lives on API version v2 of the listen endpoint. This full segment replaces
    // whatever /v{n} API version the configured BaseAddress carries (the SDK default is v1),
    // following the same pattern the Agent client uses for its own endpoint.
    public const string LISTEN_V2 = "v2/listen";
}
