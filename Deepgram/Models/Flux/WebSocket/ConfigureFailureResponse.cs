// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Sent by the Deepgram Flux (v2 listen) API when a Configure message could not be
/// applied. The session continues with its previous configuration.
/// All properties are optional: the AsyncAPI spec defines {type, request_id, sequence_id} while
/// the docs show {type, sequence_id, code, description}. // [verify] spec/docs disagree; this type accepts both
/// </summary>
public record ConfigureFailureResponse
{
    /// <summary>
    /// ConfigureFailure event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FluxType? Type { get; set; } = FluxType.ConfigureFailure;

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
    /// A string code describing the failure, when provided by the API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Prose description of the failure, when provided by the API.
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
