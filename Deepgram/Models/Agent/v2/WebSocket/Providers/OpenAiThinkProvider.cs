// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// OpenAI think provider. Assign to Think.Provider.
///
/// Required by the API: <c>Type</c> (defaulted) and <c>Model</c>. Unset properties are omitted from
/// the payload, so a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record OpenAiThinkProvider
{
    /// <summary>
    /// Provider type. Always "open_ai".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "open_ai";

    /// <summary>
    /// The REST API version for the OpenAI chat completions API. Accepted value: "v1".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// OpenAI model to use, e.g. "gpt-5", "gpt-4.1" or "gpt-4o-mini".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// OpenAI temperature, 0 to 2.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Reasoning effort. One of "none", "minimal", "low", "medium" or "high".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("reasoning_mode")]
    public string? ReasoningMode { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
