// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// Request body for updating the value of an existing template variable. The key cannot be
/// changed after creation.
/// </summary>
public class UpdateAgentVariableSchema
{
    /// <summary>
    /// REQUIRED. The new value to substitute. Can be any valid JSON type (string, number,
    /// boolean, object, or array).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
