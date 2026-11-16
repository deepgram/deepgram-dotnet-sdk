// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Configures Deepgram Think Live, which runs alongside the reasoning LLM for backchannels,
/// presence checks, and frustration detection.
/// </summary>
public record ThinkLiveSettings
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("provider")]
    public DeepgramThinkLiveProvider? Provider { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
