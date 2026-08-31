// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.AgentManage.v1;

/// <summary>
/// The list of reusable agent configurations for a project. The documented shape is an object
/// with an "agents" array, but the live API currently returns a bare array; both are accepted.
/// </summary>
[JsonConverter(typeof(AgentConfigurationsResponseConverter))]
public record AgentConfigurationsResponse
{
    /// <summary>
    /// A list of agent configurations for the project.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("agents")]
    public List<AgentConfigurationResponse>? Agents { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}

/// <summary>
/// Accepts both list shapes: the documented {"agents":[...]} object and the bare [...] array
/// the live API currently returns. Always serializes to the documented object shape.
/// </summary>
public class AgentConfigurationsResponseConverter : JsonConverter<AgentConfigurationsResponse>
{
    public override AgentConfigurationsResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return new AgentConfigurationsResponse
            {
                Agents = JsonSerializer.Deserialize<List<AgentConfigurationResponse>>(ref reader, options),
            };
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
            doc.RootElement.TryGetProperty("agents", out var agents) &&
            agents.ValueKind == JsonValueKind.Array)
        {
            return new AgentConfigurationsResponse
            {
                Agents = agents.Deserialize<List<AgentConfigurationResponse>>(options),
            };
        }

        return new AgentConfigurationsResponse();
    }

    public override void Write(Utf8JsonWriter writer, AgentConfigurationsResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Agents != null)
        {
            writer.WritePropertyName("agents");
            JsonSerializer.Serialize(writer, value.Agents, options);
        }
        writer.WriteEndObject();
    }
}
