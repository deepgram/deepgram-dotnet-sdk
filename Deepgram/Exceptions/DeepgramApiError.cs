using System.Text.Json;

namespace Deepgram.Exceptions;
public class DeepgramApiError : DeepgramError
{
    public int Status;
    public const string Name = "DeepgramApiError";
    public DeepgramApiError(string message, int status) : base(message)
    {
        Status = status;
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}
