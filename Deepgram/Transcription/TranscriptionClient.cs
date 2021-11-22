using System;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Transcription
{
    internal class TranscriptionClient : ITranscriptionClient
    {
        private CleanCredentials _credentials;

        public TranscriptionClient(CleanCredentials credentials)
        {
            _credentials = credentials;
        }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Url source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetPrerecordedTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions? options)
        {
            return await ApiRequest.DoRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                new Uri(_credentials.ApiUrl, "/v1/listen"),
                _credentials,
                options,
                source);
        }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetPrerecordedTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions? options)
        {
            return await ApiRequest.DoStreamRequestAsync<PrerecordedTranscription>(
                HttpMethod.Post,
                new Uri(_credentials.ApiUrl, "/v1/listen"),
                _credentials,
                source,
                options);
        }
    }
}
