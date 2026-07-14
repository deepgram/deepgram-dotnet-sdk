// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

using Common = Deepgram.Models.Common.v2.WebSocket;

/// <summary>
/// (PREVIEW) Raised locally by the SDK when the Flux (v2 speak) WebSocket connection closes.
/// </summary>
public record CloseResponse : Common.CloseResponse
{
}
