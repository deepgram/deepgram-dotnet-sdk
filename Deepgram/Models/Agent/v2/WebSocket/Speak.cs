// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

public record Speak
{
    /// <summary>
    /// The provider configuration for the TTS.
    /// For backward compatibility, this can be a single provider object.
    /// For new array format, use the SpeakProviders property instead.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("provider")]
    public dynamic Provider { get; set; } = new Provider();

    /// <summary>
    /// Custom endpoint for custom models - to use a custom model, set provider.type to the flavour of API you are using (e.g. open_ai for OpenAI-like APIs).
    /// Note: This is for backward compatibility with single provider format.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("endpoint")]
    public Endpoint? Endpoint { get; set; } = null;

    /// <summary>
    /// Array of speak provider configurations. Each provider can have its own provider settings and endpoint.
    /// When this property is set, it takes precedence over the single Provider and Endpoint properties.
    /// This supports the new array format: [{"provider": {...}, "endpoint": {...}}, ...]
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speak")]
    public List<SpeakProviderConfig>? SpeakProviders { get; set; } = null;

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}

/// <summary>
/// Configuration for a single speak provider in the array format
/// </summary>
public record SpeakProviderConfig
{
    /// <summary>
    /// The provider configuration for this specific TTS provider
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("provider")]
    public dynamic Provider { get; set; } = new Provider();

    /// <summary>
    /// Custom endpoint for this specific provider
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
