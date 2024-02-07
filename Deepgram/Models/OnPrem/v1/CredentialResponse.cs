// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public record CredentialResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
