using System;
using System.Threading.Tasks;
using Deepgram.Common;

namespace Deepgram.Projects
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
    }
}
