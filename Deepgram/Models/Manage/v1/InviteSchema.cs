// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class InviteSchema
{
    /// <summary>
    /// email of the person being invited
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// scopes to add for the invited person
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

}
