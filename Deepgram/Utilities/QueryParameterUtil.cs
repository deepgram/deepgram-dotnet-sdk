// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections;
using Deepgram.Models.PreRecorded.v1;
using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Speak.v1;

namespace Deepgram.Utilities;

internal static class QueryParameterUtil
{
    /// <summary>
    /// Formats a URL with the specified parameters
    /// </summary>
    public static string FormatURL<S>(string uriSegment, S? parameter, Dictionary<string, string>? addons = null)
    {
        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        Regex regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(uriSegment))
            uriSegment = $"https://{uriSegment}";

        // schema to be used to build the query string
        var queryString = "";
        if (parameter != null)
        {
            var propertyInfoList = parameter.GetType()
                .GetProperties()
                .Where(v => v.GetValue(parameter) is not null);

            queryString = UrlEncode(parameter, propertyInfoList, addons);
        } else
        {
            queryString = UrlEncode(parameter, null, addons);
        }

        // otherwise build the string
        var uriBuilder = new UriBuilder(uriSegment);
        var query = HttpUtility.ParseQueryString(queryString);
        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Encodes the specified parameters into a URL
    /// </summary>
    internal static string UrlEncode<T>(T parameters, IEnumerable<PropertyInfo>? propertyInfoList, Dictionary<string, string>? addons = null)
    {
        var sb = new StringBuilder();
        if (propertyInfoList != null)
        {
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
                if (typeof(T) == typeof(AnalyzeSchema) && string.Compare(name, nameof(AnalyzeSchema.CallBack).ToLower(), StringComparison.Ordinal) == 0)
                {
                    sb.Append($"{name}={HttpUtility.UrlEncode(pValue.ToString())}&");
                    continue;
                }
                if (typeof(T) == typeof(SpeakSchema) && string.Compare(name, nameof(SpeakSchema.CallBack).ToLower(), StringComparison.Ordinal) == 0)
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
