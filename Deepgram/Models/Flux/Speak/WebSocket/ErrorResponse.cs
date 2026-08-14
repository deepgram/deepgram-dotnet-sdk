// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// A fatal, server-originated error from the Deepgram Flux (v2 speak) API. The wire
/// value of the "type" property is literally "Error". Always followed by a WebSocket close. Codes
/// use Deepgram's DOMAIN-NNNN convention (e.g. MESSAGE-0000, NET-0000).
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Error event type. The wire value is "Error".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.Error;

    /// <summary>
    /// A code identifying the error, e.g. MESSAGE-0000 or NET-0000.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Prose description of the error.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
