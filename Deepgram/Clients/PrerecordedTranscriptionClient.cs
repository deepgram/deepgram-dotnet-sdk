namespace Deepgram.Clients;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>")]
public class PrerecordedTranscriptionClient : IPrerecordedTranscriptionClient
{
    private ApiRequest _apiRequest;
    internal PrerecordedTranscriptionClient(ApiRequest apiRequest)
    {
        _apiRequest = apiRequest;
    }
    /// <summary>
    /// Submits a request to the Deepgram API to transcribe prerecorded audio
    /// </summary>
    /// <param name="source">Url source to send for transcription</param>
    /// <param name="options">Feature options for the transcription</param>
    /// <returns>Transcription of the provided audio</returns>
    public async Task<PrerecordedTranscription> GetTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions options)
        => await _apiRequest.SendHttpRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                source,
                options);

    /// <summary>
    /// Submits a request to the Deepgram API to transcribe prerecorded audio
    /// </summary>
    /// <param name="source">Audio source to send for transcription</param>
    /// <param name="options">Feature options for the transcription</param>
    /// <returns>Transcription of the provided audio</returns>
    public async Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options)
        => await _apiRequest.SendHttpRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                source,
                options);
}
