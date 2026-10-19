// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Deepgram text-to-speech provider. Aura models use version "v1" (the default);
/// Flux TTS uses version "v2" and a flux-* model. Assign to Speak.Provider.
///
/// Required by the API: <c>Type</c> (defaulted) and <c>Model</c>. Unset properties are omitted from
/// the payload, so a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record DeepgramSpeakProvider
{
    /// <summary>
    /// Provider type. Always "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// The Deepgram text-to-speech model family. Accepted values: "v1" (Aura, the
    /// default) and "v2" (Flux TTS). Use "v1" with an aura-* model and "v2" with a
    /// flux-* model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Deepgram voice model, e.g. "aura-2-thalia-en" or "flux-kit-en".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Speaking-rate multiplier. Aura (version v1) accepts 0.7 to 1.5. Flux TTS
    /// (version v2) accepts 0.5 to 1.5 in 0.05 increments; a value the family does
    /// not accept ends the session with FAILED_TO_SPEAK.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }

    /// <summary>
    /// Delivery register of the generated speech, on a calm-to-animated axis.
    /// Flux TTS (version v2) only. Accepts the whole numbers -2 to 2, where 0 is
    /// the voice's tuned delivery.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("expressivity")]
    public int? Expressivity { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
