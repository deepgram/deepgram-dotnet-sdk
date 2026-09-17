// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Vestigial type from an older function-parameter model that only allowed a single hardcoded
/// <c>item</c> property. It is not used by <see cref="Function"/>, whose <c>Parameters</c> is a
/// <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> you populate with a JSON Schema
/// containing as many named properties as you need (see #367). Kept only for source compatibility.
/// </summary>
[Obsolete("Function.Parameters is a Dictionary<string, object>; build a JSON Schema there with as many named properties as you need. See #367.")]
public record Properties
{
#pragma warning disable CS0618 // Item is obsolete; this vestigial type still references it.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("item")]
    public Item? Item { get; set; }
#pragma warning restore CS0618


    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
