// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Deepgram speech-to-text provider using the V2 API (Flux STT).
/// Assign to Listen.Provider.
///
/// Required by the API: <c>Type</c> (defaulted) and <c>Model</c>. Unset properties are omitted from
/// the payload, so a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record DeepgramListenProviderV2
{
    /// <summary>
    /// Provider type for speech-to-text. Always "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Specifies usage of the V2 Deepgram speech-to-text API, e.g. Flux STT.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; } = "v2";

    /// <summary>
    /// Model to use for speech to text using the V2 API, e.g. "flux-general-en"
    /// or "flux-general-multi".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// One or more BCP-47 language codes to bias the model toward specific languages.
    /// Only supported when Model is flux-general-multi. Without hints the model
    /// auto-detects the spoken language.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_hints")]
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// End-of-turn confidence required to finish a turn. Valid range 0.5 to 1.0,
    /// default 0.7. Set to 1.0 to fully suppress confidence-based end-of-turn
    /// detection; EotTimeoutMs still ends idle turns.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_threshold")]
    public double? EotThreshold { get; set; }

    /// <summary>
    /// End-of-turn confidence required to fire an eager end-of-turn event. When set,
    /// enables EagerEndOfTurn and TurnResumed events. Valid range 0.3 to 0.9.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eager_eot_threshold")]
    public double? EagerEotThreshold { get; set; }

    /// <summary>
    /// A turn finishes when this many milliseconds have passed after speech,
    /// regardless of end-of-turn confidence. Defaults to 5000.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_timeout_ms")]
    public int? EotTimeoutMs { get; set; }

    /// <summary>
    /// Prompt keyterm recognition to improve Keyword Recall Rate.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
