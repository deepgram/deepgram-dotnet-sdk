// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.WebSocket;

/// <summary>
/// Updates selected settings on an open Listen v1 stream.
/// </summary>
public class ConfigureSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = "Configure";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? Features { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
