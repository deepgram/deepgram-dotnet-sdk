// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Member
{
    /// <summary>
    /// Unique identifier of member
    /// </summary>
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    /// <summary>
    /// First name of member
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of member
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }


    /// <summary>
    /// Email address of member
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Scopes of the key
    /// </summary>
    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }
}
