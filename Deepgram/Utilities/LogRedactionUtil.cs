// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json.Nodes;

namespace Deepgram.Utilities;

/// <summary>
/// Masks credential-bearing fields before a payload is written to the log.
///
/// The Voice Agent Settings payload is the one connect payload that carries secrets: provider
/// credentials (AWS access keys and STS session tokens) and per-provider endpoint headers, which
/// is where a third-party bearer token for OpenAI, Anthropic or ElevenLabs is supplied. Settings
/// is logged on connect, and <see cref="Library.Initialize(Logger.LogLevel, string?)"/> writes
/// log.txt by default, so an unredacted payload puts those secrets on disk.
/// </summary>
internal static class LogRedactionUtil
{
    internal const string RedactedPlaceholder = "***REDACTED***";

    /// <summary>
    /// Property names whose value is a credential wherever they appear.
    /// </summary>
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "access_key_id",
        "secret_access_key",
        "session_token",
    };

    /// <summary>
    /// Property names whose nested object holds caller-supplied values that are credentials by
    /// convention. Every string value inside is masked, while the keys are kept so the shape of
    /// the request stays debuggable.
    /// </summary>
    private static readonly HashSet<string> SensitiveContainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "headers",
    };

    /// <summary>
    /// Serializes <paramref name="payload"/> and returns it with every credential-bearing value
    /// replaced by <see cref="RedactedPlaceholder"/>. Never throws: if the payload cannot be
    /// serialized or walked, a placeholder is returned rather than risking an unredacted dump.
    /// </summary>
    public static string SerializeRedacted(object? payload)
    {
        if (payload is null)
        {
            return "null";
        }

        try
        {
            var node = JsonNode.Parse(JsonSerializer.Serialize(payload, JsonSerializeOptions.DefaultOptions));
            if (node is null)
            {
                return "null";
            }

            Redact(node);
            return node.ToJsonString(JsonSerializeOptions.DefaultOptions);
        }
        catch (Exception)
        {
            // A logging helper must never break the call it is logging, and must never fall back
            // to emitting the raw payload.
            return "<unserializable payload, redacted>";
        }
    }

    private static void Redact(JsonNode node)
    {
        switch (node)
        {
            case JsonObject obj:
                // Materialize first: assigning to an indexer while enumerating invalidates it.
                foreach (var name in obj.Select(property => property.Key).ToList())
                {
                    var value = obj[name];

                    if (SensitiveKeys.Contains(name) && value is JsonValue)
                    {
                        obj[name] = RedactedPlaceholder;
                        continue;
                    }

                    if (SensitiveContainers.Contains(name) && value is JsonObject container)
                    {
                        foreach (var headerName in container.Select(header => header.Key).ToList())
                        {
                            container[headerName] = RedactedPlaceholder;
                        }

                        continue;
                    }

                    if (value is not null)
                    {
                        Redact(value);
                    }
                }

                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        Redact(item);
                    }
                }

                break;
        }
    }
}
