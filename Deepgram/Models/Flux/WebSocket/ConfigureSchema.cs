// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Mid-stream reconfiguration message for the Deepgram Flux (v2 listen) API.
/// Any property left null keeps its currently configured value. Note that a supplied
/// Keyterms list REPLACES the entire active keyterm list (it is not merged).
/// The API answers with ConfigureSuccess or ConfigureFailure.
/// <see href="https://developers.deepgram.com/docs/flux/configure"/>
/// </summary>
public class ConfigureSchema
{
    /// <summary>
    /// Message type. Always "Configure".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "Configure";

    /// <summary>
    /// End-of-turn detection thresholds to update. Unsupplied thresholds keep their current values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("thresholds")]
    public ConfigureThresholds? Thresholds { get; set; }

    /// <summary>
    /// Key terms to boost. Replaces the ENTIRE currently active keyterm list. Maximum 100 terms.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Language hints. Only valid when the model is flux-general-multi. An empty list clears
    /// the hints; null keeps the current hints.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_hints")]
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
