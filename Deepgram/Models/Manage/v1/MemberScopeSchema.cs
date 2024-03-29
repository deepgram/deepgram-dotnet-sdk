﻿// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class MemberScopeSchema(string scope)
{
    /// <summary>
    /// Scope to add for member
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; } = scope;
}
