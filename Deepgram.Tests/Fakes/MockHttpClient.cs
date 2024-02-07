// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string? url = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            url = $"https://{Defaults.DEFAULT_URI}";
        }

        return new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri(url)
        };
    }


    public static HttpClient CreateHttpClientWithException(Exception Exception)
    {
        return new HttpClient(new MockHttpMessageHandlerException(Exception))
        {
            BaseAddress = new Uri($"https://{Defaults.DEFAULT_URI}")
        };
    }
}
