// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.v2.WebSocket;

/// <summary>
/// (PREVIEW) Describes the current turn and latest state of the turn.
/// Sent by the Deepgram Flux (v2 listen) API.
/// <see href="https://developers.deepgram.com/docs/flux/state"/>
/// </summary>
public record TurnInfoResponse
{
    /// <summary>
    /// TurnInfo event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FluxType? Type { get; set; } = FluxType.TurnInfo;

    /// <summary>
    /// The unique identifier of the request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Starts at 0 and increments for each message the server sends to the client,
    /// including messages of other types.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sequence_id")]
    public int? SequenceId { get; set; }

    /// <summary>
    /// The turn lifecycle event as sent on the wire: StartOfTurn, Update, EagerEndOfTurn,
    /// TurnResumed, or EndOfTurn. Kept as a string so unrecognized future values are preserved;
    /// use <see cref="EventType"/> for the parsed enum value.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    /// <summary>
    /// The parsed <see cref="TurnEvent"/> for <see cref="Event"/>, or null when the wire value
    /// is missing or not recognized by this SDK version.
    /// </summary>
    [JsonIgnore]
    public TurnEvent? EventType
    {
        get
        {
            if (Event != null && Enum.TryParse<TurnEvent>(Event, out var parsed))
            {
                return parsed;
            }
            return null;
        }
    }

    /// <summary>
    /// The index of the current turn. Increments immediately following an EndOfTurn event.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("turn_index")]
    public int? TurnIndex { get; set; }

    /// <summary>
    /// Start time in seconds of the audio range that was transcribed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("audio_window_start")]
    public double? AudioWindowStart { get; set; }

    /// <summary>
    /// End time in seconds of the audio range that was transcribed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("audio_window_end")]
    public double? AudioWindowEnd { get; set; }

    /// <summary>
    /// Text that was said over the course of the current turn.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    /// <summary>
    /// The words in the transcript. Word-level start/end timestamps may be omitted by the API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("words")]
    public List<Word>? Words { get; set; }

    /// <summary>
    /// Confidence that no more speech is coming in this turn.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("end_of_turn_confidence")]
    public double? EndOfTurnConfidence { get; set; }

    /// <summary>
    /// Detected languages sorted by descending frequency. Only present when the
    /// flux-general-multi model detects languages in the audio.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("languages")]
    public List<string>? Languages { get; set; }

    /// <summary>
    /// The language hints that were supplied for this turn. Only present when language hints
    /// are configured (flux-general-multi model).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("languages_hinted")]
    public List<string>? LanguagesHinted { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
