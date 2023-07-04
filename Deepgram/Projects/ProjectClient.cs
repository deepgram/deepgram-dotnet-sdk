using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Projects
{
    public sealed class ProjectClient : BaseClient, IProjectClient
    {
        internal ProjectClient(CleanCredentials credentials) : base(credentials) { }

        /// <summary>
        /// Returns all Deepgram projects
        /// </summary>
        /// <returns>List of Deepgram projects</returns>
        public async Task<ProjectList> ListProjectsAsync()
        {
            return await ApiRequest.DoRequestAsync<ProjectList>(
                HttpMethod.Get,
                "projects",
                Credentials
            );
        }

        /// <summary>
        /// Retrieves the Deepgram project associated with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve</param>
        /// <returns>A Deepgram project</returns>
        public async Task<Project> GetProjectAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<Project>(
                HttpMethod.Get,
                $"projects/{projectId}",
                Credentials
            );
        }

        /// <summary>
        /// Updates the name and company name of a Deepgram project
        /// </summary>
        /// <param name="project">Project to update</param>
        /// <returns>A message denoting the success of the operation</returns>
        public async Task<MessageResponse> UpdateProjectAsync(Project project)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
#if NETSTANDARD2_0
                new HttpMethod("PATCH"),
#else
                HttpMethod.Patch,
#endif
                $"projects/{project.Id}",
                Credentials,
                project
            );
        }

        /// <summary>
        /// Deletes a project with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        public async Task<MessageResponse> DeleteProjectAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}",
                Credentials);
        }

        /// <summary>
        /// Returns all members of a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to get members.</param>
        /// <returns>List of members</returns>
        public async Task<MemberList> GetMembersAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<MemberList>(
                       HttpMethod.Get,
                       $"projects/{projectId}/members",
                       Credentials
                   );
        }

        /// <summary>
        /// Returns member scopes for the specific project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <returns>List of member scopes</returns>
        public async Task<ScopesList> GetMemberScopesAsync(string projectId, string memberId)
        {
            return await ApiRequest.DoRequestAsync<ScopesList>(
                HttpMethod.Get,
                $"projects/{projectId}/members/{memberId}/scopes",
                Credentials
                );
        }

        /// Removes the authenticated account from the specified project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to remove the authenticated account</param>
        public async Task<MessageResponse> LeaveProjectAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/projects/{projectId}/leave",
                Credentials);
        }

        /// <summary>
        /// Removes a member from a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        public async Task<MessageResponse> RemoveMemberAsync(string projectId, string memberId)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/projects/{projectId}/members/{memberId}",
                Credentials);
        }

        /// <summary>
        /// Updates member scopes on a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <param name="options">Scope options to update</param>
        public async Task<MessageResponse> UpdateScopeAsync(string projectId, string memberId, UpdateScopeOptions options)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Put,
                $"projects/projects/{projectId}/members/{memberId}/scopes",
                Credentials,
                options);
        }
    }
}
