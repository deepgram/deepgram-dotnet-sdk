using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deepgram.Utilities
{
    internal static class QueryParameterUtil

    {
        public static string GetParameters(object parameters = null)
        {
            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();

            if (parameters != null)
            {

                var json = JsonConvert.SerializeObject(parameters);
                var jObj = (JObject)JsonConvert.DeserializeObject(json);

                foreach (var prop in jObj.Properties())
                {
                    if (prop.HasValues && !String.IsNullOrEmpty(prop.Value.ToString()))
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
                        else if (prop.Value.Type == JTokenType.Float)
                        {
                            paramList.Add(new KeyValuePair<string, string>(prop.Name, ((JValue)prop.Value).ToString(CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            paramList.Add(new KeyValuePair<string, string>(prop.Name, HttpUtility.UrlEncode(prop.Value.ToString())));
                        }
                    }
                }

            }

            return String.Join("&", paramList.Select(s => $"{s.Key}={s.Value.ToString()}")).ToLower();
        }
    }
}
