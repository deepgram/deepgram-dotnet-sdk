namespace Deepgram.Models;

public class Utterance
{
    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal End { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this word.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }

    /// <summary>
    /// Audio channel to which the utterance belongs.
    /// </summary>
    [JsonPropertyName("channel")]
    public int Channel { get; set; }

    /// <summary>
    /// Transcript for the audio segment being processed.
    /// </summary>
    [JsonPropertyName("transcript")]
    public string Transcript { get; set; }

    /// <summary>
    /// Integer indicating the speaker who is saying the word being processed.
    /// </summary>
    [JsonPropertyName("speaker")]
    public int? Speaker { get; set; }

    /// <summary>
    /// Unique identifier of the utterance
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Object containing each word in the transcript, along with its start time
    /// and end time(in seconds) from the beginning of the audio stream, and a confidence value.
    /// </summary>
    [JsonPropertyName("words")]
    public Words[] Words { get; set; }

}
