// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Utilities;

internal static class RequestContentUtil
{
    public const string DEFAULT_CONTENT_TYPE = "application/json";

    static ILogger logger => LogProvider.GetLogger(nameof(RequestContentUtil));
    static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Create the body payload of a HttpRequestMessage
    /// </summary>
    /// <typeparam name="T">Type of the body to be sent</typeparam>
    /// <param name="body">instance value for the body</param>
    /// <param name="contentType">What type of content is being sent default is : application/json</param>
    /// <returns></returns>
    internal static StringContent CreatePayload<T>(T body)
    {
        var serialized = JsonSerializer.Serialize(body, _jsonSerializerOptions);
        return new(serialized, Encoding.UTF8, DEFAULT_CONTENT_TYPE);
    }


    /// <summary>
    /// Create the stream payload of a HttpRequestMessage
    /// </summary>
    /// <param name="body">of type stream</param>
    /// <returns>HttpContent</returns>
    internal static HttpContent CreateStreamPayload(Stream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        HttpContent httpContent = new StreamContent(body);
        httpContent.Headers.Add("Content-Length", body.Length.ToString());
        return httpContent;
    }


    /// <summary>
    /// method that deserializes DeepgramResponse and performs null checks on values
    /// </summary>
    /// <typeparam name="TResponse">Class Type of expected response</typeparam>
    /// <param name="httpResponseMessage">Http Response to be deserialized</param>       
    /// <returns>instance of TResponse or a Exception</returns>
    internal static async Task<TResponse> DeserializeAsync<TResponse>(HttpResponseMessage httpResponseMessage)
    {
        var content = await httpResponseMessage.Content.ReadAsStringAsync();
        var deepgramResponse = JsonSerializer.Deserialize<TResponse>(content);
        return deepgramResponse;
    }

}
