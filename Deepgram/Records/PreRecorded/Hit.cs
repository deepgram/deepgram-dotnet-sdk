﻿namespace Deepgram.Records.PreRecorded;

public record Hit
{
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("snippet")]
    public string Snippet { get; set; }
}