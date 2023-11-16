using System.Text;
using System.Web;
using Deepgram.Models.Schemas;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    public static string GetParameters<T>(T parameters)
    {
        var sb = new StringBuilder();

        if (parameters is not null)
        {
            foreach (var pInfo in parameters.GetType().GetProperties())
            {
                var pValue = pInfo.GetValue(parameters);
                if (pValue is not null)
                {
                    var name = pInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
                    if (pInfo.PropertyType.IsArray)
                    {
                        foreach (var value in (Array)pValue)
                            sb.Append($"{name}={HttpUtility.UrlEncode(value.ToString().ToLower())}&");
                    }
                    else if (pValue is DateTime)
                    {
                        sb.Append($"{name}={HttpUtility.UrlEncode(((DateTime)pValue).ToString("yyyy-MM-dd"))}&");
                    }
                    else if (typeof(T) == typeof(PrerecordedSchema) && name == "callback")
                    {
                        sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                    }
                    else
                    {
                        sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString().ToLower())}&");
                    }
                }
            }
        }

        return sb.ToString().Trim('&');
    }
}
