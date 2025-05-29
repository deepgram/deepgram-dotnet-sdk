// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

public record Speak
{
    /// <summary>
    /// The provider for the TTS.
    /// Must be "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("provider")]
    public dynamic Provider { get; set; } = new SpeakProvider();

    /// <summary>
    /// Custom endpoint for custom models - to use a custom model, set provider.type to the flavour of API you are using (e.g. open_ai for OpenAI-like APIs).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("endpoint")]
    public Endpoint? Endpoint { get; set; } = null;

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
