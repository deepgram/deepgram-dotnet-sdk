namespace Deepgram.Exceptions;
public class DeepgramApiError(string message, int status) : DeepgramError(message)
{
    public int Status = status;
    public const string Name = "DeepgramApiError";

    public string ToJson() => JsonSerializer.Serialize(this);
}
