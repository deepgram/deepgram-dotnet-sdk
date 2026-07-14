// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Sent by the Deepgram Flux (v2 listen) API when a Configure message was applied
/// successfully. Echoes the currently active configuration.
/// <see href="https://developers.deepgram.com/docs/flux/configure"/>
/// </summary>
public record ConfigureSuccessResponse
{
    /// <summary>
    /// ConfigureSuccess event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FluxType? Type { get; set; } = FluxType.ConfigureSuccess;

    /// <summary>
    /// The unique identifier of the request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Starts at 0 and increments for each message the server sends to the client,
    /// including messages of other types.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sequence_id")]
    public int? SequenceId { get; set; }

    /// <summary>
    /// The currently active end-of-turn thresholds.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("thresholds")]
    public ConfigureThresholds? Thresholds { get; set; }

    /// <summary>
    /// The currently active keyterms. The API may send a single string or an array of strings;
    /// both are read into a list.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keyterms")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string>? Keyterms { get; set; }

    /// <summary>
    /// The currently active language hints. Only applicable to the flux-general-multi model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_hints")]
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
