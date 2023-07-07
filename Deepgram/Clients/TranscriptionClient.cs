namespace Deepgram.Clients;

public class TranscriptionClient : ITranscriptionClient
{
    public IPrerecordedTranscriptionClient Prerecorded { get; protected set; }

    public TranscriptionClient(IApiRequest apiRequest) => Prerecorded = new PrerecordedTranscriptionClient(apiRequest);
}
