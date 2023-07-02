using System.Web;
using Newtonsoft.Json.Linq;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil

{
    public static string GetParameters(object? parameters = null)
    {
        List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();

        if (parameters is not null)
        {

            var json = JsonConvert.SerializeObject(parameters);

            if (json is not null)
            {
                var jObj = JsonConvert.DeserializeObject(json) as JObject;

                foreach (var prop in jObj.Properties())
                {
                    if (prop.HasValues && !string.IsNullOrEmpty(prop.Value.ToString()))
                    {
                        if (prop.Value.Type == JTokenType.Array)
                        {
                            foreach (var value in prop.Values())
                            {
                                paramList.Add(new KeyValuePair<string, string>(prop.Name, HttpUtility.UrlEncode(value.ToString())));
                            }
                        }
                        else if (prop.Value.Type == JTokenType.Date)
                        {
                            paramList.Add(new KeyValuePair<string, string>(prop.Name, HttpUtility.UrlEncode(((DateTime)prop.Value).ToString("yyyy-MM-dd"))));
                        }
                        else
                        {
                            paramList.Add(new KeyValuePair<string, string>(prop.Name, HttpUtility.UrlEncode(prop.Value.ToString())));
                        }
                    }
                }
            }
        }

        return string.Join("&", paramList.Select(s => $"{s.Key}={s.Value}")).ToLower();
    }
}
