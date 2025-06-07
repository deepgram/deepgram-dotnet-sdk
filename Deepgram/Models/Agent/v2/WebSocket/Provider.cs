using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Dynamic;

namespace Deepgram.Models.Agent.v2.WebSocket;

public class Provider: DynamicObject
{
    /// <summary>
    /// The provider for the service.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Arbitrary additional properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var name = binder.Name;
        if (AdditionalProperties != null && AdditionalProperties.TryGetValue(name, out var value))
        {
            result = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => value.ToString()
            };
            return true;
        }
        result = null;
        return false;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        // Convert the property name to snake_case
        var name = ToSnakeCase(binder.Name);
        if (AdditionalProperties == null)
            AdditionalProperties = new Dictionary<string, JsonElement>();

        var json = JsonSerializer.Serialize(value);
        AdditionalProperties[name] = JsonDocument.Parse(json).RootElement;
        return true;
    }

    // Helper method for snake_case conversion
    private static string ToSnakeCase(string name)
    {
        return string.Concat(
            name.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x) : char.ToLower(x).ToString()
            )
        );
    }

    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
