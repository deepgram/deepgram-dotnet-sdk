// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record ModelsResponse
{
    /// <summary>
    /// Contains the Speech-to-Text models
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stt")]
    public List<Stt>? Stt { get; set; }

    /// <summary>
    /// Contains the Text-to-Speech models
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tts")]
    public List<Tts>? Tts { get; set; }
}
