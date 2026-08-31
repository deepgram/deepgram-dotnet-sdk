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
    /// <summary>
    /// The unique identifier of the variable.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("variable_id")]
    public string? VariableId { get; set; }

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
    /// Timestamp when the variable was created.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the variable was last updated.
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
