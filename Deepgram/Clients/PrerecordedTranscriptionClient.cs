using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    public class PrerecordedTranscriptionClient : BaseClient, IPrerecordedTranscriptionClient
    {
        public PrerecordedTranscriptionClient(Credentials credentials) : base(credentials) { }
        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Url source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetTranscriptionAsync(UrlSource source, PrerecordedTranscriptionOptions options)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
               HttpMethod.Post,
                "listen",
                Credentials,
                source,
                options);

            return await ApiRequest.SendHttpRequestAsync<PrerecordedTranscription>(req);
        }

        /// <summary>
        /// Submits a request to the Deepgram API to transcribe prerecorded audio
        /// </summary>
        /// <param name="source">Audio source to send for transcription</param>
        /// <param name="options">Feature options for the transcription</param>
        /// <returns>Transcription of the provided audio</returns>
        public async Task<PrerecordedTranscription> GetTranscriptionAsync(StreamSource source, PrerecordedTranscriptionOptions options)
        {
            var req = RequestMessageBuilder.CreateStreamHttpRequestMessage(
             HttpMethod.Post,
              "listen",
              Credentials,
              source,
              options);

            return await ApiRequest.SendHttpRequestAsync<PrerecordedTranscription>(req);
        }
    }
}
