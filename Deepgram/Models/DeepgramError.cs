namespace Deepgram.Models;
public class DeepgramError : Exception
{
    // Properties of DeepgramError go here

    public DeepgramError(string message, Exception exception) : base(message, exception)
    {

    }
    public DeepgramError(string message) : base(message)
    {

    }

    public DeepgramError()
    {

    }
}
