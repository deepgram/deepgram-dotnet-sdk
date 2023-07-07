namespace Deepgram.Clients;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>")]
public class PrerecordedTranscriptionClient : BaseClient, IPrerecordedTranscriptionClient
{
    public PrerecordedTranscriptionClient(CleanCredentials credentials) : base(credentials) { }
    /// <summary>
    /// Submits a request to the Deepgram API to transcribe prerecorded audio
    /// </summary>
    /// <param name="source">Url source to send for transcription</param>
    /// <param name="options">Feature options for the transcription</param>
    /// <returns>Transcription of the provided audio</returns>
    public async Task<PrerecordedTranscription> GetTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions options)
        => await ApiRequest.SendHttpRequestAsync<PrerecordedTranscription>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Post,
                "listen",
                Credentials,
                source,
                options));

    /// <summary>
    /// Submits a request to the Deepgram API to transcribe prerecorded audio
    /// </summary>
    /// <param name="source">Audio source to send for transcription</param>
    /// <param name="options">Feature options for the transcription</param>
    /// <returns>Transcription of the provided audio</returns>
    public async Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options)
        => await ApiRequest.SendHttpRequestAsync<PrerecordedTranscription>(
            RequestMessageBuilder.CreateStreamHttpRequestMessage(
                HttpMethod.Post,
                "listen",
                Credentials,
                source,
                options));
}
