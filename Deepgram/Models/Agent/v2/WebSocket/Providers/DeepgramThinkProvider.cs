// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Deepgram think provider. Assign to Think.Provider.
/// </summary>
public record DeepgramThinkProvider
{
    /// <summary>
    /// Provider type. Always "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Deepgram model to use, such as "think-fast".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
