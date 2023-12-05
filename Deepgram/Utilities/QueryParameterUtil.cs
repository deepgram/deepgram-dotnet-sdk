using System.Web;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    public static string GetParameters<T>(T parameters)
    {
        try
        {
            if (parameters is null) return string.Empty;

            var propertyInfoList = parameters.GetType()
                .GetProperties()
                .Where(v => v.GetValue(parameters) is not null);
            return UrlEncode(parameters, propertyInfoList);

        }
        catch (Exception ex)
        {
            var type = typeof(T).Name;
            Log.ParameterStringError(LogProvider.GetLogger(nameof(QueryParameterUtil)), type, ex);
            throw new Exception($"Error creating a query string from object of type {type} ", ex);
        }
    }

    private static string UrlEncode<T>(T parameters, IEnumerable<PropertyInfo> propertyInfos)
    {
        var sb = new StringBuilder();
        foreach (var pInfo in propertyInfos)
        {
            var name = pInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            var pValue = pInfo.GetValue(parameters);

            switch (pValue)
            {
                case Array array:
                    foreach (var value in array)
                        sb.Append($"{name}={HttpUtility.UrlEncode(value.ToString())}&");
                    break;
                case DateTime time:
                    sb.Append($"{name}={HttpUtility.UrlEncode(time.ToString("yyyy-MM-dd"))}&");
                    break;
                default:
                    sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                    break;
            }
        }
        return sb.ToString();
    }
}
