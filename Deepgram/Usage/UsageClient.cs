using System;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Usage
{
    internal class UsageClient : IUsageClient
    {
        private CleanCredentials _credentials;

        public UsageClient(CleanCredentials credentials)
        {
            _credentials = credentials;
        }

        /// <summary>
        /// Generates a list of requests sent to the Deepgram API for the specified project over a given time range. 
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>Usage Requests that fit the parameters provided</returns>
        public async Task<ListAllRequestsResponse> ListAllRequestsAsync(string projectId, ListAllRequestsOptions options)
        {
            return await ApiRequest.DoRequestAsync<ListAllRequestsResponse>(    
                HttpMethod.Get,
                new Uri(_credentials.ApiUrl, $"/v1/projects/{projectId}/requests"),
                _credentials,
                options
            );
        }

        /// <summary>
        /// Returns details about a specific request to the Deepgram API
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="requestId">Unique identifier of the request to retrieve</param>
        /// <returns>Usage Request identified</returns>
        public async Task<UsageRequest> GetUsageRequestAsync(string projectId, string requestId)
        {
            return await ApiRequest.DoRequestAsync<UsageRequest>(
                HttpMethod.Get,
                new Uri(_credentials.ApiUrl, $"/v/projects/{projectId}/requests/{requestId}"),
                _credentials
            );
        }

        /// <summary>
        /// Retrieves a summary of usage statistics. 
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>Summary of usage statistics</returns>
        public async Task<UsageSummary> GetUsageSummaryAsync(string projectId, GetUsageSummaryOptions options)
        {
            return await ApiRequest.DoRequestAsync<UsageSummary>(
                HttpMethod.Get,
                new Uri(_credentials.ApiUrl, $"/v1/projects/{projectId}/usage"),
                _credentials,
                options
            );
        }

        /// <summary>
        /// Retrieves a list of features, models, tags, languages, and processing method used for requests in the specified project.
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to report on</param>
        /// <param name="options">Pagination & filtering options</param>
        /// <returns>List of features, models, tags, languages, and processing method used for requests in the specified project.</returns>
        public async Task<UsageFields> GetUsageFieldsAsync(string projectId, GetUsageFieldsOptions options)
        {
            return await ApiRequest.DoRequestAsync<UsageFields>(
                HttpMethod.Get,
                new Uri(_credentials.ApiUrl, $"/v1/projects/{projectId}/usage/fields"),
                _credentials,
                options
            );
        }
    }
}
