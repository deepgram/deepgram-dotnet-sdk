namespace Deepgram.Clients;

public class UsageClient : BaseClient, IUsageClient
{

    public UsageClient(CleanCredentials credentials) : base(credentials) { }

    /// <summary>
    /// Generates a list of requests sent to the Deepgram API for the specified project over a given time range. 
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to report on</param>
    /// <param name="options">Pagination & filtering options</param>
    /// <returns>Usage Requests that fit the parameters provided</returns>
    public async Task<ListAllRequestsResponse> ListAllRequestsAsync(string projectId, ListAllRequestsOptions options)
        => await ApiRequest.SendHttpRequestAsync<ListAllRequestsResponse>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/requests",
                Credentials,
                null,
                options));

    /// <summary>
    /// Returns details about a specific request to the Deepgram API
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to report on</param>
    /// <param name="requestId">Unique identifier of the request to retrieve</param>
    /// <returns>Usage Request identified</returns>
    public async Task<UsageRequest> GetUsageRequestAsync(string projectId, string requestId)
        => await ApiRequest.SendHttpRequestAsync<UsageRequest>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/requests/{requestId}",
                Credentials));

    /// <summary>
    /// Retrieves a summary of usage statistics. 
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to report on</param>
    /// <param name="options">Pagination & filtering options</param>
    /// <returns>Summary of usage statistics</returns>
    public async Task<UsageSummary> GetUsageSummaryAsync(string projectId, GetUsageSummaryOptions options)
        => await ApiRequest.SendHttpRequestAsync<UsageSummary>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/usage",
                Credentials,
                null,
                options));

    /// <summary>
    /// Retrieves a list of features, models, tags, languages, and processing method used for requests in the specified project.
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to report on</param>
    /// <param name="options">Pagination & filtering options</param>
    /// <returns>List of features, models, tags, languages, and processing method used for requests in the specified project.</returns>
    public async Task<UsageFields> GetUsageFieldsAsync(string projectId, GetUsageFieldsOptions options)
        => await ApiRequest.SendHttpRequestAsync<UsageFields>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/usage/fields",
                Credentials,
                null,
                options));
}
