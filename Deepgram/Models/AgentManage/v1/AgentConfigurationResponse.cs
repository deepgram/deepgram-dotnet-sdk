// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// A reusable agent configuration stored with Deepgram. The returned AgentId can be passed
/// in place of the full agent object in a Voice Agent Settings message.
/// <see href="https://developers.deepgram.com/docs/reusable-agent-configurations"/>
/// </summary>
public record AgentConfigurationResponse
{
    private string? _agentId;

    /// <summary>
    /// The unique identifier of the agent configuration. The documented field name is
    /// agent_id, but the live API currently returns it as agent_uuid; this property returns
    /// whichever the API sent (agent_id preferred when both are present).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agent_id")]
    public string? AgentId { get => _agentId ?? AgentUuid; set => _agentId = value; }

    /// <summary>
    /// The identifier as currently returned on the wire by the live API. Prefer
    /// <see cref="AgentId"/>, which falls back to this value automatically.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agent_uuid")]
    public string? AgentUuid { get; set; }

    /// <summary>
    /// The agent configuration (the agent block of a Settings message), in its uninterpolated
    /// form: template variable placeholders appear as-is. The documented shape is a parsed JSON
    /// object, but the live API currently returns the stored JSON-encoded string; check
    /// <see cref="JsonElement.ValueKind"/> to see which arrived.
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
    /// API version of the stored configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("api_version")]
    public int? ApiVersion { get; set; }

    /// <summary>
    /// Timestamp when the configuration was created. May be absent (the live API does not
    /// currently return it).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the configuration was last updated. May be absent (the live API does not
    /// currently return it).
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
