using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Dynamic;

namespace Deepgram.Models.Agent.v2.WebSocket;

public class SpeakProvider: DynamicObject
{
    /// <summary>
    /// The provider for the TTS.
    /// Must be one of "deepgram", "eleven_labs", "cartesia", or "open_ai".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Deepgram OR OpenAI model to use.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Eleven Labs OR Cartesia model to use.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Cartesia voice configuration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("voice")]
    public CartesiaVoice? Voice { get; set; } = null;

    /// <summary>
    /// Optional Cartesia Language
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Optional Eleven labs language.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_id")]
    public string? LanguageId { get; set; }

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
        var name = binder.Name;
        if (AdditionalProperties == null)
            AdditionalProperties = new Dictionary<string, JsonElement>();

        // Convert value to JsonElement
        var json = JsonSerializer.Serialize(value);
        AdditionalProperties[name] = JsonDocument.Parse(json).RootElement;
        return true;
    }

    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
