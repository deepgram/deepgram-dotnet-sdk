// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Translation
{
    /// <summary>
    /// Translated transcript.
    /// </summary>
    [JsonPropertyName("translation")]
    public string? TranslatedTranscript { get; set; }

    /// <summary>
    /// Language code of the translation.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

