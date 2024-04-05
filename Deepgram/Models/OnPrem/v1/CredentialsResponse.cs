// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public record CredentialsResponse
{
    /// <summary>
    /// Distribution credentials
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("distribution_credentials")]
    public IReadOnlyList<CredentialResponse>? DistributionCredentials { get; set; }
}
