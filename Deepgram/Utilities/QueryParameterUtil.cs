// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Linq;
using Deepgram.Models.PreRecorded.v1;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    internal static string GetParameters<T>(T parameters, Dictionary<string, string>? addons = null)
    {
        if (parameters is null) return string.Empty;

        var propertyInfoList = parameters.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameters) is not null);


        return UrlEncode(parameters, propertyInfoList, addons);
    }

    internal static string AppendQueryParameters(string uriSegment, Dictionary<string, string>? parameters = null)
    {
        if (parameters is null)
        {
            return uriSegment;
        }

        // otherwise build the string
        var uriBuilder = new UriBuilder(uriSegment);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in parameters)
        {
            query[param.Key] = param.Value;
        }

        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }

    private static string UrlEncode<T>(T parameters, IEnumerable<PropertyInfo> propertyInfoList, Dictionary<string, string>? addons = null)
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
                case bool boolean:
                    sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString().ToLower())}&");
                    break;
                case IList list:
                    foreach (var value in list)
                        sb.Append($"{name}={HttpUtility.UrlEncode(value.ToString())}&");
                    break;
                case DateTime time:
                    sb.Append($"{name}={HttpUtility.UrlEncode(time.ToString("yyyy-MM-dd"))}&");
                    break;
                //specific case for the Extra Parameter dictionary to format the querystring correctly
                //no case changing of the key or values as theses are unknowns and the casing may have 
                //significance to the user
                case Dictionary<string, string> dict:
                    if (name == "extra")
                    {
                        foreach (var kvp in dict)
                        {
                            sb.Append($"{name}={HttpUtility.UrlEncode($"{kvp.Key}:{kvp.Value}")}&");
                        }
                    }
                    break;
                default:
                    sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                    break;
            }
        }

        if (addons != null)
        {
            foreach (var kvp in addons)
            {
                sb.Append($"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}&");
            }
        }

        return sb.ToString().TrimEnd('&');
    }
}
