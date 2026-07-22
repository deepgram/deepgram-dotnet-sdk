// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) A single word within a Flux <see cref="TurnInfoResponse"/> transcript.
/// </summary>
public record Word
{
    /// <summary>
    /// The individual punctuated, properly-cased word from the transcript.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("word")]
    public string? HeardWord { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this word.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// May be omitted by the API. // [verify] optional per AsyncAPI spec and all sibling SDKs
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("start")]
    public double? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word ends.
    /// May be omitted by the API. // [verify] optional per AsyncAPI spec and all sibling SDKs
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("end")]
    public double? End { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
