// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Callback
{
    /// <summary>
    /// Attempt number.
    /// </summary>
    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    /// <summary>
    /// HTTP status code returned by the callback, indicating the result of the callback execution.
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// Is Completed
    /// </summary>
    [JsonPropertyName("completed")]
    public string? Completed { get; set; }
}
