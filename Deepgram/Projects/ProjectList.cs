using System;
using System.Text.Json.Serialization;

namespace Deepgram.Projects
{
    public class ProjectList
    {
        /// <summary>
        /// List of Deepgram projects
        /// </summary>
        [JsonPropertyName("projects")]
        public List<Project> Projects { get; set; }

    }
}
