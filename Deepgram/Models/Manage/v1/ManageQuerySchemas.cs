// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

/// <summary>
/// Optional pagination filters for a project lookup.
/// </summary>
public class ProjectQuerySchema
{
    public int? Limit { get; set; }
    public int? Page { get; set; }
}

/// <summary>
/// Optional status filter for a project's API keys.
/// </summary>
public class KeysQuerySchema
{
    public string? Status { get; set; }
}
