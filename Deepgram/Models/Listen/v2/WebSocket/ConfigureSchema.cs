// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v2.WebSocket;

/// <summary>
/// Updates selected settings on an open <c>/v1/listen</c> stream. Keyterms require a Nova-3
/// model and the global endpoint. A successful update does not receive an acknowledgement; the
/// server sends an Error response when it rejects a Configure message. Keep keyterms under the
/// 500-token limit: an over-limit update can stop transcription and close with <c>1011 (NET-0000)</c>
/// without an Error response.
/// </summary>
public class ConfigureSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = "Configure";

    /// <summary>
    /// Replaces the current keyterm list. An empty list clears keyterms; <c>null</c> leaves them unchanged.
    /// Keep each update under the 500-token keyterm limit.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Updates supported formatting features, such as <c>numerals</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? Features { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
