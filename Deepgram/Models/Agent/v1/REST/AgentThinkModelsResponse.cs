// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v1.REST;

/// <summary>
/// The Think models currently available for Voice Agent sessions.
/// </summary>
public class AgentThinkModelsResponse
{
    [JsonPropertyName("models")]
    public List<AgentThinkModel>? Models { get; set; }
}

/// <summary>
/// A Think model advertised by the Voice Agent catalog.
/// </summary>
public class AgentThinkModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}
