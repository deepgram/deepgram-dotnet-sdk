// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Mid-stream reconfiguration message for the Deepgram Flux (v2 speak) API. Changes the speaking
/// rate without reconnecting. Not validated client-side — an out-of-range value is rejected by the
/// server via <see cref="ConfigureFailureResponse"/> (e.g. code SPEED_OUT_OF_RANGE), not thrown locally.
/// <see href="https://developers.deepgram.com/docs/flux-tts/client-messages"/>
/// </summary>
public class ConfigureSchema
{
    /// <summary>
    /// Message type. Always "Configure".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "Configure";

    /// <summary>
    /// Speech-rate multiplier. Valid values are 0.85 through 1.15 in 0.05 increments. Applies at
    /// the next segment boundary. Not range-checked client-side.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
