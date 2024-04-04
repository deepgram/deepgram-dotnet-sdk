// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public record DistributionCredentials
{
    /// <summary>
    /// Distribution credentials ID
    /// </summary>
    [JsonPropertyName("distribution_credentials_id")]
    public string? DistributionCredentialsId { get; set; }

    /// <summary>
    /// Provider name
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Comment
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Scopes of the credentials
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>
    /// Created date/time
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
}
