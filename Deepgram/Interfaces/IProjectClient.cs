using System;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Models;

namespace Deepgram.Interfaces
{
    public interface IProjectClient
    {
        /// <summary>
        /// Returns all Deepgram projects
        /// </summary>
        /// <returns>List of Deepgram projects</returns>
        Task<ProjectList> ListProjectsAsync();

        /// <summary>
        /// Retrieves the Deepgram project associated with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve</param>
        /// <returns>A Deepgram project</returns>
        Task<Project> GetProjectAsync(string projectId);

        /// <summary>
        /// Updates the name and company name of a Deepgram project
        /// </summary>
        /// <param name="project">Project to update</param>
        /// <returns>A message denoting the success of the operation</returns>
        Task<MessageResponse> UpdateProjectAsync(Project project);

        /// <summary>
        /// Deletes a project with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        Task<MessageResponse> DeleteProjectAsync(string projectId);

        /// <summary>
        /// Returns all members of a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to get members.</param>
        /// <returns>List of members of a project</returns>
        Task<MemberList> GetMembersAsync(string projectId);

        /// <summary>
        /// Returns member scopes for the specific project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project.</param>
        /// <param name="memberId">Unique identifier of the member.</param>
        /// <returns>List of member scopes</returns>
        Task<ScopesList> GetMemberScopesAsync(string projectId, string memberId);

        /// Removes the authenticated account from the specified project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to remove the authenticated account</param>
        Task<MessageResponse> LeaveProjectAsync(string projectId);

        /// <summary>
        /// Removes a member from a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        Task<MessageResponse> RemoveMemberAsync(string projectId, string memberId);

        /// <summary>
        /// Updates member scopes on a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <param name="options">Scope options to update</param>
        Task<MessageResponse> UpdateScopeAsync(string projectId, string memberId, UpdateScopeOptions options);
    }
}
