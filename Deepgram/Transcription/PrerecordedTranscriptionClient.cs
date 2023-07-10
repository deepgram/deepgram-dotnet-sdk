using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Transcription
{
    public sealed class PrerecordedTranscriptionClient : IPrerecordedTranscriptionClient
    {

        private ApiRequest _apiRequest;
        public PrerecordedTranscriptionClient(ApiRequest apiRequest)
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
        {
            return await _apiRequest.DoRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                source,
                options);
        }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options)
        {
            return await _apiRequest.DoStreamRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                "listen",
                source,
                options);
        }
    }
}
