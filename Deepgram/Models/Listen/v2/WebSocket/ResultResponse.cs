// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v2.WebSocket;

public record ResultResponse
{
    /// <summary>
    /// Contains the channel information.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("channel")]
    public Channel? Channel { get; set; }

    /// <summary>
    /// Channel index.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("channel_index")]
    public IReadOnlyList<int>? ChannelIndex { get; set; }

    /// <summary>
    /// Duration of the result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("duration")]
    public decimal? Duration { get; set; }

    /// <summary>
    /// Is the result final.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("is_final")]
    public bool? IsFinal { get; set; } = false;

    /// <summary>
    /// Indicates whether this result was generated during the finalization process.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("from_finalize")]
    public bool? FromFinalize { get; set; }

    /// <summary>
    /// Metadata information.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("metadata")]
    public MetaData? MetaData { get; set; }

    /// <summary>
    /// Is the result a partial result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("speech_final")]
    public bool? SpeechFinal { get; set; }

    /// <summary>
    /// Start time of the result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Result event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ListenType? Type { get; set; } = ListenType.Results;

    // TODO: DYV is this needed???
    /// <summary>
    /// Error information.
    /// </summary>
    public Exception? Error { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
