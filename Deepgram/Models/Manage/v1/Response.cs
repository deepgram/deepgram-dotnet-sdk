// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Response
{
    /// <summary>
    /// Details of the request <see cref="Detail"/>
    /// </summary>
    [JsonPropertyName("details")]
    public Details? Details { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("completed")]
    public string? Completed { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("token_details")]
    public List<TokenDetails>? TokenDetails { get; set; }
}
