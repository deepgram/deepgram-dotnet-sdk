// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) Sent immediately by the Deepgram Flux (v2 speak) API on a successful connection.
/// Successor to the /v1/speak Metadata message.
/// </summary>
public record ConnectedResponse
{
    /// <summary>
    /// Connected event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.Connected;

    /// <summary>
    /// The unique identifier of the /v2/speak request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Resolved model name, e.g. "flux-alexis-en".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_name")]
    public string? ModelName { get; set; }

    /// <summary>
    /// Resolved model version, e.g. "2026.06.01".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_version")]
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Resolved model UUIDs. A list, because a resolved model may be backed by more than one
    /// underlying model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_uuids")]
    public List<string>? ModelUuids { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
