// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net;

namespace Deepgram.Models.Exceptions.v1;

public class DeepgramWebSocketException : DeepgramException
{
    public DeepgramWebSocketException(string errMsg) : base(errMsg)
    {
    }

    public DeepgramWebSocketException(string errMsg, Exception innerException)
        : this(errMsg, GetHttpStatusCode(innerException), innerException)
    {
    }

    public DeepgramWebSocketException(string errMsg, HttpStatusCode? httpStatusCode, Exception? innerException)
        : base(errMsg, innerException)
    {
        HttpStatusCode = httpStatusCode ?? GetHttpStatusCode(innerException);
    }

    /// <summary>
    /// HTTP status returned while upgrading the WebSocket connection, when available.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }

    internal static void EnableHttpResponseDetails(ClientWebSocket clientWebSocket)
    {
        // These properties are available on .NET 7+, but not the netstandard2.0 reference
        // assembly. Reflection keeps the SDK's multi-target build while enabling typed status
        // reporting on runtimes that provide it.
        clientWebSocket.Options.GetType().GetProperty("CollectHttpResponseDetails")?.SetValue(clientWebSocket.Options, true);
    }

    internal static HttpStatusCode? GetHttpStatusCode(ClientWebSocket clientWebSocket)
    {
        var value = clientWebSocket.GetType().GetProperty("HttpStatusCode")?.GetValue(clientWebSocket);
        return value is HttpStatusCode statusCode && statusCode != 0 ? statusCode : null;
    }

    private static HttpStatusCode? GetHttpStatusCode(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is System.Net.Http.HttpRequestException)
            {
                // StatusCode was added after netstandard2.0; reflection keeps this additive API
                // available on newer runtimes without parsing an exception message.
                if (exception.GetType().GetProperty("StatusCode")?.GetValue(exception) is HttpStatusCode statusCode)
                {
                    return statusCode;
                }
            }

            exception = exception.InnerException;
        }

        return null;
    }
}
