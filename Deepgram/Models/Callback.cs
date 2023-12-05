namespace Deepgram.Models;

public class Callback
{
    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("completed")]
    public string Completed { get; set; }
}
