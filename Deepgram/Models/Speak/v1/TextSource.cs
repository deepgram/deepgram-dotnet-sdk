// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Speak.v1;

public class TextSource(string text)
{
    /// <summary>
    /// Text of the words to speak
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; } = text;
}

