// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Turn lifecycle events reported in the "event" property of a Flux <see cref="TurnInfoResponse"/>.
/// <see href="https://developers.deepgram.com/docs/flux/state"/>
/// </summary>
public enum TurnEvent
{
    /// <summary>
    /// The user has begun speaking for the first time in the turn.
    /// </summary>
    StartOfTurn,

    /// <summary>
    /// Additional audio has been transcribed, but the turn state hasn't changed.
    /// </summary>
    Update,

    /// <summary>
    /// There is moderate confidence that the user has finished speaking. An opportunity to begin
    /// preparing an agent reply. Only sent when the eager_eot_threshold parameter is set.
    /// </summary>
    EagerEndOfTurn,

    /// <summary>
    /// Speech had appeared to end (an EagerEndOfTurn was sent), but speech is actually continuing.
    /// Only sent when the eager_eot_threshold parameter is set.
    /// </summary>
    TurnResumed,

    /// <summary>
    /// The user has finished speaking for the turn. The turn_index increments after this event.
    /// </summary>
    EndOfTurn,
}
