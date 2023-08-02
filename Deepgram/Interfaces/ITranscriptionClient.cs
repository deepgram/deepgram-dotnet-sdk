namespace Deepgram.Interfaces
{
    public interface ITranscriptionClient
    {
        IPrerecordedTranscriptionClient Prerecorded { get; }

    }
}
