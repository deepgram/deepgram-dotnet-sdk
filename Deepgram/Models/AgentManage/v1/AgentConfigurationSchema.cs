// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// Request body for creating a reusable agent configuration.
/// <see href="https://developers.deepgram.com/docs/voice-agent/configuration/reusable-configurations"/>
/// </summary>
public class AgentConfigurationSchema
{
    /// <summary>
    /// REQUIRED. A valid JSON string representing the agent block of a Voice Agent Settings
    /// message. Note this is a JSON-encoded string, not a nested object.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("config")]
    public string? Config { get; set; }

    /// <summary>
    /// A map of arbitrary key-value pairs for labeling or organizing the agent configuration.
    /// Visible to every member of the Deepgram project — do not store secrets here.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

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
