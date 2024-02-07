// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record UsageFieldsResponse
{
    /// <summary>
    /// ReadOnlyList of included tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="UsageModel"/>
    /// </summary>
    [JsonPropertyName("models")]
    public IReadOnlyList<Model>? Models { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="RequestMethod"/>
    /// </summary>
    [JsonPropertyName("processing_methods")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IReadOnlyList<RequestMethod>? ProcessingMethods { get; set; }

    /// <summary>
    /// ReadOnlyList of included languages
    /// </summary>
    [JsonPropertyName("languages")]
    public IReadOnlyList<string>? Languages { get; set; }

    /// <summary>
    /// ReadOnlyList of included features
    /// </summary>
    [JsonPropertyName("features")]
    public IReadOnlyList<string>? Features { get; set; }
}
