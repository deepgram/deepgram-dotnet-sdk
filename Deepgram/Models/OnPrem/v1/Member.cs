// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.OnPrem.v1;

public record Member
{
    /// <summary>
    /// Member ID
    /// </summary>
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    /// <summary>
    /// Email of the member
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
