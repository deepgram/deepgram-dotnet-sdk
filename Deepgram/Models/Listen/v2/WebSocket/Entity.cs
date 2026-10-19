// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v2.WebSocket;

/// <summary>
/// An entity extracted from the transcript when DetectEntities is enabled.
/// Entities are only present on final results.
/// </summary>
public record Entity
{
    /// <summary>
    /// The type or category of the entity.
    /// </summary>
    /// <remarks>e.g. NAME, PHONE_NUMBER, EMAIL_ADDRESS, ORGANIZATION, CARDINAL</remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// The formatted text representation of the entity.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// The original spoken text of the entity. Present when formatting is enabled.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("raw_value")]
    public string? RawValue { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this detected entity.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// Index of the first word of the entity in the transcript, inclusive.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// Index of the last word of the entity in the transcript, exclusive.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
