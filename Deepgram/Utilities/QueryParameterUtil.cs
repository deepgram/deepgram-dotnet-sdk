using System.Collections;
using Deepgram.Models.PreRecorded.v1;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    internal static string GetParameters<T>(T parameters)
    {
        if (parameters is null) return string.Empty;

        var propertyInfoList = parameters.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameters) is not null);
        return UrlEncode(parameters, propertyInfoList);
    }

    private static string UrlEncode<T>(T parameters, IEnumerable<PropertyInfo> propertyInfoList)
    {
        var sb = new StringBuilder();
        foreach (var pInfo in propertyInfoList)
        {
            var name = pInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            var pValue = pInfo.GetValue(parameters);

            //need to check for the CallBack property so the value  is not changed to lowercase - required by Signed uri
            if (typeof(T) == typeof(PrerecordedSchema) && string.Compare(name, nameof(PrerecordedSchema.CallBack).ToLower(), StringComparison.Ordinal) == 0)
            {
                sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                continue;
            }

            switch (pValue)
            {
                case IList list:
                    foreach (var value in list)
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
        return sb.ToString().TrimEnd('&');
    }
}
