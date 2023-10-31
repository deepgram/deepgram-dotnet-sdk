using System.Threading;
using System.Threading.Tasks;
using Deepgram.Models;

namespace Deepgram.Interfaces
{
    public interface IProjectClient : IBaseClient
    {
        /// <summary>
        /// Returns all Deepgram projects
        /// </summary>
        /// <returns>List of Deepgram projects</returns>
        Task<ProjectList> ListProjectsAsync(CancellationToken token = new CancellationToken());

        /// <summary>
        /// Retrieves the Deepgram project associated with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve</param>
        /// <returns>A Deepgram project</returns>
        Task<Project> GetProjectAsync(string projectId, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Updates the name and company name of a Deepgram project
        /// </summary>
        /// <param name="project">Project to update</param>
        /// <returns>A message denoting the success of the operation</returns>
        Task<MessageResponse> UpdateProjectAsync(Project project, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Deletes a project with the provided projectId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        Task<MessageResponse> DeleteProjectAsync(string projectId, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Returns all members of a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to get members.</param>
        /// <returns>List of members of a project</returns>
        Task<MemberList> GetMembersAsync(string projectId, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Returns member scopes for the specific project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project.</param>
        /// <param name="memberId">Unique identifier of the member.</param>
        /// <returns>List of member scopes</returns>
        Task<ScopesList> GetMemberScopesAsync(string projectId, string memberId, CancellationToken token = new CancellationToken());

        /// Removes the authenticated account from the specified project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to remove the authenticated account</param>
        Task<MessageResponse> LeaveProjectAsync(string projectId, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Removes a member from a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        Task<MessageResponse> RemoveMemberAsync(string projectId, string memberId, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Updates member scopes on a project
        /// </summary>
        /// <param name="projectId">Unique identifier of the project</param>
        /// <param name="memberId">Unique identifier of the member</param>
        /// <param name="options">Scope options to update</param>
        Task<MessageResponse> UpdateScopeAsync(string projectId, string memberId, UpdateScopeOptions options, CancellationToken token = new CancellationToken());
    }
}
