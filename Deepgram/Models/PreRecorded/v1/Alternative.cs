// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Alternative
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Entity"/> objects.
    /// </summary>
    /// <remark>Only used when the detect entities feature is enabled on the request</remark>
    [JsonPropertyName("entities")]
    public IReadOnlyList<Entity>? Entities { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="ParagraphGroup"/> containing  separated transcript and <see cref="Paragraph"/> objects.
    /// </summary>
    /// <remark>Only used when the paragraph feature is enabled on the request</remark>
    [JsonPropertyName("paragraphs")]
    public ParagraphGroup? Paragraphs { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="Summary "/> objects.
    /// </summary>
    /// <remark>Only used when the summarize feature is enabled on the request</remark>
    [JsonPropertyName("summaries")]
    public IReadOnlyList<SummaryObsolete>? Summaries { get; set; }

    /// <summary>
    /// Single-string transcript containing what the model hears in this channel of audio.
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }


    /// <summary>
    /// ReadOnlyList of <see cref="Translation"/> objects.
    /// </summary>
    /// <remark>Only used when the translation feature is enabled on the request</remark>
    [JsonPropertyName("translations")]
    public IReadOnlyList<Translation>? Translations { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="Word"/> objects.
    /// </summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<Word>? Words { get; set; }
}
