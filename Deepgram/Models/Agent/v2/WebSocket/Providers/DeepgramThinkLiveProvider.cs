// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// The Deepgram Think Live provider. Assign to <see cref="ThinkLiveSettings.Provider"/>.
/// </summary>
public record DeepgramThinkLiveProvider
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; } = "v1";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agent_backchannel_sensitivity")]
    public double? AgentBackchannelSensitivity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("presence_check_max_count")]
    public int? PresenceCheckMaxCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("presence_check_interval_ms")]
    public int? PresenceCheckIntervalMs { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("frustration_max_count")]
    public int? FrustrationMaxCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("restricted_terms")]
    public List<string>? RestrictedTerms { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
