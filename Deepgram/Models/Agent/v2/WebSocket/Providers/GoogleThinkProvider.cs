// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Google think provider. Assign to Think.Provider.
///
/// Required by the API: <c>Type</c> (defaulted) and <c>Model</c>. Unset properties are omitted from
/// the payload, so a missing required field is rejected by the server rather than caught locally.
/// </summary>
public record GoogleThinkProvider
{
    /// <summary>
    /// Provider type. Always "google".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "google";

    /// <summary>
    /// The Google API used for the request: "ai-studio-v1beta" for the AI Studio API,
    /// or "gemini-enterprise-agent-v1" for the Gemini Enterprise Agent (GEA) API.
    /// "v1beta" is accepted as an alias for "ai-studio-v1beta". Defaults based on the
    /// Deepgram Voice Agent endpoint you connect to.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Google model to use, e.g. "gemini-2.0-flash", "gemini-2.0-flash-lite"
    /// or "gemini-2.5-flash".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Google temperature, 0 to 2.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
