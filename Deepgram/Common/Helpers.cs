using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

namespace Deepgram.Common
{
    internal static class Helpers
    {
        public static string GetUserAgent()
        {
#if NETSTANDARD1_6 || NETSTANDARD2_0 || NETSTANDARD2_1
            // TODO: watch the next core release; may have functionality to make this cleaner
            var languageVersion = (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Replace(" ", "")
                .Replace("/", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("_", "")
                .Replace("(", "")
                .Replace(")", "")
                ;
#else
            var languageVersion = System.Diagnostics.FileVersionInfo
                .GetVersionInfo(typeof(int).Assembly.Location)
                .ProductVersion;
#endif
            var libraryVersion = typeof(Helpers)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            return $"deepgram/{libraryVersion} dotnet/{languageVersion}";
        }

        public static Dictionary<string, string> GetParameters(object parameters)
        {
            var json = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }

}
