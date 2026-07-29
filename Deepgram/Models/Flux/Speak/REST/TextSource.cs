// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.REST;

/// <summary>
/// (PREVIEW) The request body for the Deepgram Flux (v2 speak) batch REST API. Serializes to
/// {"text":"..."}. The server normalizes and preprocesses the text before synthesis.
/// </summary>
public class TextSource(string text)
{
    /// <summary>
    /// The text content to be converted to speech.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("text")]
    public string? Text { get; set; } = text;

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
