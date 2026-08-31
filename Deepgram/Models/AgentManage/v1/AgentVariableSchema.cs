// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// Request body for creating a template variable.
/// <see href="https://developers.deepgram.com/docs/voice-agent/configuration/reusable-configurations"/>
/// </summary>
public class AgentVariableSchema
{
    /// <summary>
    /// REQUIRED. The variable name, following the DG_&lt;VARIABLE_NAME&gt; format.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// REQUIRED. The value to substitute. Can be any valid JSON type (string, number, boolean,
    /// object, or array). Visible to every member of the Deepgram project — do not store
    /// secrets here.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// API version. Defaults to 1 when not set.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("api_version")]
    public int? ApiVersion { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
