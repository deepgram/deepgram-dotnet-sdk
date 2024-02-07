// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record MetadataResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model_info")]
    public IReadOnlyDictionary<Guid, ModelInfo>? ModelInfo { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("models")]
    public IReadOnlyList<string>? Models { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("transaction_key")]
    public string? TransactionKey { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType? Type { get; set; } = LiveType.Metadata;

    /// <summary>
    /// Deepgram’s Extra Metadata feature allows you to attach arbitrary key-value pairs to your API requests that are attached to the API response for usage in downstream processing.
    /// Extra metadata is limited to 2048 characters per key-value pair.
    /// <see href="https://developers.deepgram.com/docs/extra-metadata"/>
    /// </summary>
    [JsonPropertyName("extra")]
    public Dictionary<string, string>? Extra { get; set; }
}
