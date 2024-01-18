﻿namespace Deepgram.Records.Live;

public record Word
{
    /// <summary>
    /// Distinct word heard by the model.
    /// </summary>
    [JsonPropertyName("word")]
    public string? HeardWord { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this word.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonPropertyName("punctuated_word")]
    public string PunctuatedWord { get; set; }
}