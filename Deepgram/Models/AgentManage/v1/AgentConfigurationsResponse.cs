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
/// Accepts exactly two list shapes: the documented {"agents":[...]} object and the bare [...]
/// array the live API currently returns. Any other shape (unknown envelope, non-array "agents",
/// scalar, null) throws <see cref="JsonException"/> so API contract drift fails visibly instead
/// of silently looking like a project with no configurations. Always serializes to the
/// documented object shape.
/// </summary>
public class AgentConfigurationsResponseConverter : JsonConverter<AgentConfigurationsResponse>
{
    // Ensure Read is invoked for a JSON null so it throws rather than silently producing a
    // null response that is indistinguishable from an empty project.
    public override bool HandleNull => true;

    public override AgentConfigurationsResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return new AgentConfigurationsResponse
            {
                Agents = JsonSerializer.Deserialize<List<AgentConfigurationResponse>>(ref reader, options),
            };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty("agents", out var agents) &&
                agents.ValueKind == JsonValueKind.Array)
            {
                return new AgentConfigurationsResponse
                {
                    Agents = agents.Deserialize<List<AgentConfigurationResponse>>(options),
                };
            }

            throw new JsonException(
                "Unrecognized agent configurations list shape: expected an object with an \"agents\" array or a bare array.");
        }

        throw new JsonException(
            $"Unrecognized agent configurations list shape: expected an object with an \"agents\" array or a bare array, got {reader.TokenType}.");
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
