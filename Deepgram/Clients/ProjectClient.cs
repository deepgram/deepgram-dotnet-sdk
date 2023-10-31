using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Utilities;

namespace Deepgram.Clients
{
    public class ProjectClient : BaseClient, IProjectClient
    {
        public ProjectClient(Credentials credentials, HttpClientUtil httpClientUtil)
            : base(credentials, httpClientUtil) { }

        /// <summary>
        /// Returns all Deepgram projects
        /// </summary>
        /// <returns>List of Deepgram projects</returns>
        public async Task<ProjectList> ListProjectsAsync(CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
            "projects",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<ProjectList>(req, token);
        }

        /// <summary>
        /// Retrieves the Deepgram project associated with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve</param>
        /// <returns>A Deepgram project</returns>
        public async Task<Project> GetProjectAsync(string projectId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
            $"projects/{projectId}",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<Project>(req, token);

        }

        /// <summary>
        /// Updates the name and company name of a Deepgram project
        /// </summary>
        /// <param name="project">Project to update</param>
        /// <returns>A message denoting the success of the operation</returns>
        public async Task<MessageResponse> UpdateProjectAsync(Project project, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
#if NETSTANDARD2_0
                new HttpMethod("PATCH"),
#else
                HttpMethod.Patch,
#endif
                $"projects/{project.Id}",
                Credentials,
               project);

            return await ApiRequest.SendHttpRequestAsync<MessageResponse>(req, token);


        }

        /// <summary>
        /// Deletes a project with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        public async Task<MessageResponse> DeleteProjectAsync(string projectId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Delete,
            $"projects/{projectId}",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<MessageResponse>(req, token);

        }

        /// <summary>
        /// Returns all members of a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to get members.</param>
        /// <returns>List of members</returns>
        public async Task<MemberList> GetMembersAsync(string projectId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
             $"projects/{projectId}/members",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<MemberList>(req, token);

        }

        /// <summary>
        /// Returns member scopes for the specific project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <returns>List of member scopes</returns>
        public async Task<ScopesList> GetMemberScopesAsync(string projectId, string memberId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
             $"projects/{projectId}/members/{memberId}/scopes",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<ScopesList>(req, token);
        }

        /// Removes the authenticated account from the specified project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to remove the authenticated account</param>
        public async Task<MessageResponse> LeaveProjectAsync(string projectId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
             HttpMethod.Delete,
                $"projects/{projectId}/leave",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<MessageResponse>(req, token);

        }

        /// <summary>
        /// Removes a member from a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        public async Task<MessageResponse> RemoveMemberAsync(string projectId, string memberId, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Delete,
                $"projects/{projectId}/members/{memberId}",
            Credentials);

            return await ApiRequest.SendHttpRequestAsync<MessageResponse>(req, token);

        }

        /// <summary>
        /// Updates member scopes on a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <param name="options">Scope options to update</param>
        public async Task<MessageResponse> UpdateScopeAsync(string projectId, string memberId, UpdateScopeOptions options, CancellationToken token = new CancellationToken())
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Put,
            $"projects/{projectId}/members/{memberId}/scopes",
            Credentials,
            options);

            return await ApiRequest.SendHttpRequestAsync<MessageResponse>(req, token);

        }
    }
}
