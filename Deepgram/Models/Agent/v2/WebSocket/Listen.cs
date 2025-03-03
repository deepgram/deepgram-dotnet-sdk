// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

public record Listen
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Keyterm Prompting allows you improve Keyword Recall Rate (KRR) for important keyterms or phrases up to 90%.
    /// <see href="https://developers.deepgram.com/docs/keyterm">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
