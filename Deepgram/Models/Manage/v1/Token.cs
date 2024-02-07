// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Token
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("in")]
    public int? In { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("out")]
    public int? Out { get; set; }
}
