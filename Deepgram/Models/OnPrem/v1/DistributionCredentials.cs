// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public record DistributionCredentials
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("distribution_credentials_id")]
    public string? DistributionCredentialsId { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
}
