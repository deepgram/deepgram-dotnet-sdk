// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record UsageSummaryResponse
{
    /// <summary>
    /// Start date for included requests.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date for included requests.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Resolution of the usage <see cref="v1.Resolution"/>
    /// </summary>
    [JsonPropertyName("resolution")]
    public Resolution? Resolution { get; set; }

    /// <summary>
    /// Result summaries <see cref="UsageSummary"/>
    /// </summary>
    [JsonPropertyName("results")]
    public IReadOnlyList<Result>? Results { get; set; }
}
