// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Emitted by the Deepgram Flux (v2 speak) API rejecting a client Configure message. Non-fatal —
/// the connection stays open. <see cref="Code"/> is an open, growable set of values (e.g.
/// SPEED_OUT_OF_RANGE); do not assume it is limited to any known list.
/// </summary>
public record ConfigureFailureResponse
{
    /// <summary>
    /// ConfigureFailure event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.ConfigureFailure;

    /// <summary>
    /// A code identifying why the configuration was rejected, e.g. SPEED_OUT_OF_RANGE. Open set —
    /// the server may introduce new codes without notice.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Prose description of why the configuration was rejected.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The rejected field's name, e.g. "speed".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>
    /// The rejected value, echoed back verbatim. Untyped because the field it names may vary.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
