// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

using Common = Deepgram.Models.Common.v2.WebSocket;

/// <summary>
/// (PREVIEW) Raised when the Flux API sends a message type this SDK version does not recognize.
/// The Raw property contains the original JSON.
/// </summary>
public record UnhandledResponse : Common.UnhandledResponse
{
}
