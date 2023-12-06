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

            //need to check for the CallBack property so the value  is not changed to lowercase 
            if (typeof(T) == typeof(PrerecordedSchema) && name == nameof(PrerecordedSchema.Callback).ToLower())
            {
                sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                continue;
            }

            switch (pValue)
            {
                case Array array:
                    foreach (var value in array)
                        sb.Append($"{name}={HttpUtility.UrlEncode(value.ToString().ToLower())}&");
                    break;
                case DateTime time:
                    sb.Append($"{name}={HttpUtility.UrlEncode(time.ToString("yyyy-MM-dd"))}&");
                    break;
                default:
                    sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString().ToLower())}&");
                    break;
            }
        }
        return sb.ToString();
    }
}
