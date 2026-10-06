// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.SelfHosted.v1;

/// <summary>
/// Query options for creating current self-hosted distribution credentials.
/// </summary>
public class DistributionCredentialsCreateOptions
{
    /// <summary>
    /// Permission scopes for the new credentials. Each scope is sent as a separate query parameter.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// Distribution service provider. The hosted API currently supports <c>quay</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}
