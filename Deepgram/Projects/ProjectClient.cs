using System;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Projects
{
    internal class ProjectClient : IProjectClient
    {
        private CleanCredentials _credentials;

        public ProjectClient(CleanCredentials credentials)
        {
            _credentials = credentials;
        }

        /// <summary>
        /// Returns all Deepgram projects
        /// </summary>
        /// <returns>List of Deepgram projects</returns>
        public async Task<ProjectList> ListProjectsAsync()
        {
            return await ApiRequest.DoRequestAsync<ProjectList>(    
                HttpMethod.Get,
                "/v1/projects",
                _credentials
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
                $"/v1/projects/{projectId}",
                _credentials
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
                $"/v1/projects/{project.Id}",
                _credentials,
                null,
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
                $"/v1/projects/{projectId}",
                _credentials);
        }
    }
}
