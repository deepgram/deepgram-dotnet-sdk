using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    public class PrerecordedTranscriptionClient : BaseClient, IPrerecordedTranscriptionClient
    {
        public PrerecordedTranscriptionClient(Credentials credentials) : base(credentials) { }
        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<PrerecordedTranscriptionCallbackResult> GetTranscriptionAsync(UrlSource source, string callbackUrl, PrerecordedTranscriptionOptions options)
        {
            _ = callbackUrl ?? throw new System.ArgumentNullException(nameof(callbackUrl), "Callback is required");

            options.Callback = callbackUrl;

            var req = RequestMessageBuilder.CreateHttpRequestMessage(
             HttpMethod.Post,
             "listen",
             Credentials,
             source,
             options);
            return await ApiRequest.SendHttpRequestAsync<PrerecordedTranscriptionCallbackResult>(req);
        }

        /// <inheritdoc />
        public async Task<PrerecordedTranscriptionCallbackResult> GetTranscriptionAsync(StreamSource source, string callbackUrl, PrerecordedTranscriptionOptions options)
        {
            _ = callbackUrl ?? throw new System.ArgumentNullException(nameof(callbackUrl), "Callback is required");

            options.Callback = callbackUrl;

            var req = RequestMessageBuilder.CreateStreamHttpRequestMessage(
             HttpMethod.Post,
             "listen",
             Credentials,
             source,
             options);
            return await ApiRequest.SendHttpRequestAsync<PrerecordedTranscriptionCallbackResult>(req);
        }
    }
}
