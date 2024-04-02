// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record Summary
{
    /// <summary>
    /// Summary of a section of the transcript
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

