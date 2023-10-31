using System.Threading;
using System.Threading.Tasks;
using Deepgram.Models;

namespace Deepgram.Interfaces
{
    public interface IPrerecordedTranscriptionClient : IBaseClient
    {
        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Url source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        Task<PrerecordedTranscription> GetTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions options, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Asynchronously submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Url source to send for transcription</param>
        /// <param name="callbackUrl">Url to send the transcription results to</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        Task<PrerecordedTranscriptionCallbackResult> GetTranscriptionAsync(UrlSource source, string callbackUrl, PrerecordedTranscriptionOptions options, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Asynchronously submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="callbackUrl">Url to send the transcription results to</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        Task<PrerecordedTranscriptionCallbackResult> GetTranscriptionAsync(StreamSource source, string callbackUrl, PrerecordedTranscriptionOptions options, CancellationToken token = new CancellationToken());

    }
}
