// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record ParagraphGroup
{
    /// <summary>
    /// Full transcript
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Paragraph"/>
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public IReadOnlyList<Paragraph>? Paragraphs { get; set; }
}

