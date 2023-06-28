using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    public class UsageClient : BaseClient, IUsageClient
    {

        public UsageClient(Credentials credentials) : base(credentials) { }

        /// <summary>
        /// Generates a list of requests sent to the Deepgram API for the specified project over a given time range. 
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>Usage Requests that fit the parameters provided</returns>
        public async Task<ListAllRequestsResponse> ListAllRequestsAsync(string projectId, ListAllRequestsOptions options)
        {
            var req = _requestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/requests",
                _credentials,
                null,
                options);

            return await _apiRequest.SendHttpRequestAsync<ListAllRequestsResponse>(req);


        }

        /// <summary>
        /// Returns details about a specific request to the Deepgram API
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="requestId">Unique identifier of the request to retrieve</param>
        /// <returns>Usage Request identified</returns>
        public async Task<UsageRequest> GetUsageRequestAsync(string projectId, string requestId)
        {
            var req = _requestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/requests/{requestId}",
                _credentials);

            return await _apiRequest.SendHttpRequestAsync<UsageRequest>(req);


        }

        /// <summary>
        /// Retrieves a summary of usage statistics. 
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>Summary of usage statistics</returns>
        public async Task<UsageSummary> GetUsageSummaryAsync(string projectId, GetUsageSummaryOptions options)
        {
            var req = _requestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                  $"projects/{projectId}/usage",
                _credentials,
                null,
                options);

            return await _apiRequest.SendHttpRequestAsync<UsageSummary>(req);

        }

        /// <summary>
        /// Retrieves a list of features, models, tags, languages, and processing method used for requests in the specified project.
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>List of features, models, tags, languages, and processing method used for requests in the specified project.</returns>
        public async Task<UsageFields> GetUsageFieldsAsync(string projectId, GetUsageFieldsOptions options)
        {
            var req = _requestMessageBuilder.CreateHttpRequestMessage(
               HttpMethod.Get,
                    $"projects/{projectId}/usage/fields",
               _credentials,
               null,
               options);

            return await _apiRequest.SendHttpRequestAsync<UsageFields>(req);

        }
    }
}
