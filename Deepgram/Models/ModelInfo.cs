﻿namespace Deepgram.Models;

public class ModelInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("arch")]
    public string? Arch { get; set; }
}