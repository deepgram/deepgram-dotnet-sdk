using System.Text;
using System.Web;

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
                            sb.Append($"{name}={HttpUtility.UrlEncode(value.ToString())}&");
                    }
                    else
                    {
                        sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                    }
                }
            }
        }

        return sb.ToString().ToLower().Trim('&');
    }
}
