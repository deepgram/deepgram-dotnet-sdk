namespace Deepgram.Models.Responses;
public class DeepgramResponse<T>
{
    public T? Result { get; set; }
    public DeepgramError? Error { get; set; }
}

