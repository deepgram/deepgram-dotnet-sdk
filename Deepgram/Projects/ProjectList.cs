using System;
using Newtonsoft.Json;

namespace Deepgram.Projects
{
    public class ProjectList
    {
        /// <summary>
        /// List of Deepgram projects
        /// </summary>
        [JsonProperty("projects")]
        public Project[] Projects { get; set; }
    }
}
