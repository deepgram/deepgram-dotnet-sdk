namespace Deepgram.Clients;

public class TranscriptionClient : ITranscriptionClient
{
    public IPrerecordedTranscriptionClient Prerecorded { get; protected set; }

    public TranscriptionClient(CleanCredentials credentials)
    {
        Prerecorded = new PrerecordedTranscriptionClient(credentials);
    }


}
