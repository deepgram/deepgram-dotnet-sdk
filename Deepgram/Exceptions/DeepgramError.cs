
namespace Deepgram.Exceptions;

[Serializable]
public class DeepgramError : Exception
{
    protected bool dgError = true;
    public DeepgramError()
    {

    }
    public DeepgramError(string message) : base(message)
    {

    }
    public DeepgramError(string message, Exception innerException) : base(message, innerException) { }

}
