﻿namespace Deepgram.Records.PreRecorded;

public record Summary
{
    [JsonPropertyName("summary")]
    public string Text { get; set; }

    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }
}
