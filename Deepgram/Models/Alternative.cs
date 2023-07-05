namespace Deepgram.Models;

public class Alternative
{
    /// <summary>
    /// Single-string transcript containing what the model hears in this channel of audio.
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }

    /// <summary>
    /// Array of Word objects.
    /// </summary>
    [JsonPropertyName("words")]
    public Words[]? Words { get; set; }

    /// <summary>
    /// <see cref="ParagraphGroup"/> containing /n seperated transcript and <see cref="Paragraph"/> objects.
    /// </summary>
    /// <remark>Only used when the paragraph feature is enabled on the request</remark>
    [JsonPropertyName("paragraphs")]
    public ParagraphGroup Paragraphs { get; set; }

    /// <summary>
    /// Array of Summary objects.
    /// </summary>
    /// <remark>Only used when the summarize feature is enabled on the request</remark>
    [JsonPropertyName("summaries")]
    public Summary[]? Summaries { get; set; }

    /// <summary>
    /// Array of Entity objects.
    /// </summary>
    /// <remark>Only used when the detect entities feature is enabled on the request</remark>
    [JsonPropertyName("entities")]
    public Entity[]? Entities { get; set; }

    /// <summary>
    /// Array of Translation objects.
    /// </summary>
    /// <remark>Only used when the translation feature is enabled on the request</remark>
    [JsonPropertyName("translations")]
    public Translation[]? Translations { get; set; }
}
