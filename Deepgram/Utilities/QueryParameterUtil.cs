using System.Data;
using System.Text.Json.Nodes;
using System.Web;
namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    public static string GetParameters(object? parameters = null)
    {
        List<(string key, string value)> paramList = new List<(string, string)>();
        if (parameters != null)
        {
            var json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions());
            if (json != null)
            {
                var obj = JsonSerializer.Deserialize<JsonObject>(json);

                foreach (var item in obj)
                {
                    if (item.Value is null) continue;

                    if (item.Value is JsonArray)
                    {
                        foreach (var value in item.Value as JsonArray)
                        {
                            paramList.Add((item.Key.ToLower(), HttpUtility.UrlEncode(value.ToString())));
                        }
                    }
                    else if (ParseDateTime(item.Value.ToString()) is not null)
                    {
                        paramList.Add((item.Key.ToLower(), HttpUtility.UrlEncode(((DateTime)item.Value).ToString("yyyy-MM-dd"))));
                    }
                    else
                    {
                        paramList.Add((item.Key.ToLower(), HttpUtility.UrlEncode(item.Value.ToString())));
                    }
                }
            }
        }
        return string.Join("&", paramList.Select(s => $"{s.key}={s.value}")).ToLowerInvariant();

    }

    public static DateTime? ParseDateTime(object value)
    {
        DateTime.TryParse(value.ToString(), out var dateTime);
        return dateTime != DateTime.MinValue ? dateTime : null;
    }

}
