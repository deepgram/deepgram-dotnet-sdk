// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Flux.Speak.WebSocket;

/// <summary>
/// URI segments for the Flux (v2 speak) text-to-speech WebSocket client.
/// </summary>
public static class UriSegments
{
    // Flux TTS lives on API version v2 of the speak endpoint. This full segment replaces
    // whatever /v{n} API version the configured BaseAddress carries (the SDK default is v1),
    // following the same pattern the Flux listen and Agent clients use.
    public const string SPEAK_V2 = "v2/speak";
}
