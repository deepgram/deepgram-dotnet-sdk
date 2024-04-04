// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record Channel
{
    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative> Alternatives { get; set; }
}
