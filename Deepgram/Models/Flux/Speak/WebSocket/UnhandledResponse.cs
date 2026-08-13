// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

using Common = Deepgram.Models.Common.v2.WebSocket;

/// <summary>
/// Raised when the Flux (v2 speak) API sends a message type this SDK version does not
/// recognize. The Raw property contains the original JSON. An unrecognized frame never throws,
/// never closes the connection, and never routes to the error event.
/// </summary>
public record UnhandledResponse : Common.UnhandledResponse
{
}
