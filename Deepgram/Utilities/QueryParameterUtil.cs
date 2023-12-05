using System.Web;
using Deepgram.Logger;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    public static string GetParameters<T>(T parameters)
    {
        try
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
                        else if (pValue is DateTime time)
                        {
                            sb.Append($"{name}={HttpUtility.UrlEncode(time.ToString("yyyy-MM-dd"))}&");
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
        catch (Exception ex)
        {
            var type = typeof(T).Name;
            Log.ParameterStringError(LogProvider.GetLogger(nameof(QueryParameterUtil)), type, ex);
            throw new Exception($"Error creating a query string from object of type {type} ", ex);
        }
    }
}
