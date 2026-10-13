// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.SelfHosted.v1;

/// <summary>
/// Request body for creating current self-hosted distribution credentials.
/// </summary>
public class DistributionCredentialsCreateSchema
{
    /// <summary>
    /// Optional human-readable description for the credentials.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
