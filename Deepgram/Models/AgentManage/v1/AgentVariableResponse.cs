// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// A template variable for reusable agent configurations. Variables follow the
/// DG_&lt;VARIABLE_NAME&gt; naming format and can substitute any JSON value in an agent
/// configuration.
/// <see href="https://developers.deepgram.com/docs/voice-agent/configuration/reusable-configurations"/>
/// </summary>
public record AgentVariableResponse
{
    private string? _variableId;

    /// <summary>
    /// The unique identifier of the variable. The documented field name is variable_id, but
    /// the live API currently returns it as agent_variable_uuid; this property returns
    /// whichever the API sent (variable_id preferred when both are present).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("variable_id")]
    public string? VariableId { get => _variableId ?? AgentVariableUuid; set => _variableId = value; }

    /// <summary>
    /// The identifier as currently returned on the wire by the live API. Prefer
    /// <see cref="VariableId"/>, which falls back to this value automatically.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agent_variable_uuid")]
    public string? AgentVariableUuid { get; set; }

    /// <summary>
    /// The variable name, following the DG_&lt;VARIABLE_NAME&gt; format.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// The value to substitute. Can be any valid JSON type (string, number, boolean, object,
    /// or array).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Whether the variable is sensitive. Currently always false; sensitive variables are not
    /// supported yet.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("is_sensitive")]
    public bool? IsSensitive { get; set; }

    /// <summary>
    /// API version of the stored variable.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("api_version")]
    public int? ApiVersion { get; set; }

    /// <summary>
    /// Timestamp when the variable was created. May be absent (the live API does not
    /// currently return it).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the variable was last updated. May be absent (the live API does not
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
