// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.v2.WebSocket;

/// <summary>
/// (PREVIEW) End-of-turn detection thresholds, used in the Flux Configure message and echoed
/// back in ConfigureSuccess. Any parameter not supplied keeps its currently configured value.
/// <see href="https://developers.deepgram.com/docs/flux/configuration"/>
/// </summary>
public record ConfigureThresholds
{
    /// <summary>
    /// End-of-turn confidence required to fire an eager end-of-turn event. When set, enables
    /// EagerEndOfTurn and TurnResumed events. Valid values 0.3 - 0.9. Must not exceed
    /// <see cref="EotThreshold"/>. Off by default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eager_eot_threshold")]
    public double? EagerEotThreshold { get; set; }

    /// <summary>
    /// End-of-turn confidence required to finish a turn. Valid values 0.5 - 0.9. Default 0.7.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_threshold")]
    public double? EotThreshold { get; set; }

    /// <summary>
    /// A turn will be finished when this much time (in milliseconds) has passed after speech,
    /// regardless of end-of-turn confidence. Valid values 500 - 10000. Default 5000.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("eot_timeout_ms")]
    public int? EotTimeoutMs { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
