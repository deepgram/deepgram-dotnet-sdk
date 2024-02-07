// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;
public record TranscriptionResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("channel")]
    public Channel? Channel { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("channel_index")]
    public IReadOnlyList<int>? ChannelIndex { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("is_final")]
    public bool? IsFinal { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("metadata")]
    public MetaData? MetaData { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("speech_final")]
    public bool? SpeechFinal { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType? Type { get; set; } = LiveType.Results;

    /// <summary>
    /// TODO
    /// </summary>
    public Exception? Error { get; set; }
}
