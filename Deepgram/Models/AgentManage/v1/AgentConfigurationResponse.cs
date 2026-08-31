// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// A reusable agent configuration stored with Deepgram. The returned AgentId can be passed
/// in place of the full agent object in a Voice Agent Settings message.
/// <see href="https://developers.deepgram.com/docs/voice-agent/configuration/reusable-configurations"/>
/// </summary>
public record AgentConfigurationResponse
{
    /// <summary>
    /// The unique identifier of the agent configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agent_id")]
    public string? AgentId { get; set; }

    /// <summary>
    /// The agent configuration object (the agent block of a Settings message). Returned in its
    /// uninterpolated form: template variable placeholders appear as-is rather than with their
    /// substituted values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("config")]
    public JsonElement? Config { get; set; }

    /// <summary>
    /// A map of arbitrary key-value pairs for labeling or organizing the agent configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Timestamp when the configuration was created. Not present on create responses.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the configuration was last updated. Not present on create responses.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
