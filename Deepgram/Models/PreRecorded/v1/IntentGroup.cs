// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record IntentGroup
{
    /// <summary> 
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// <see cref="IntentSegment"/>
    /// </summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<Segment>? Segments { get; set; }
}
