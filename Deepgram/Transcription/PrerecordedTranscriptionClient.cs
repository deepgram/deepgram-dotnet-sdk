using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Transcription
{
    public sealed class PrerecordedTranscriptionClient : BaseClient, IPrerecordedTranscriptionClient
    {

        public PrerecordedTranscriptionClient(CleanCredentials credentials) : base(credentials) { }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Url source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions options)
        {
            return await ApiRequest.DoRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                Credentials,
                source,
                options
                );
        }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options)
        {
            return await ApiRequest.DoStreamRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                Credentials,
                source,
                options);
        }
    }
}
