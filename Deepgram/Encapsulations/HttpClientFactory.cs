// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Extensions;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Encapsulations;
internal class HttpClientFactory
{
    public static HttpClientWrapper Create(string apiKey, DeepgramClientOptions deepgramClientOptions)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(Defaults.HTTPCLIENT_NAME)
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));
        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(Defaults.HTTPCLIENT_NAME);
        client.ConfigureDeepgram(apiKey, deepgramClientOptions);
        client.Timeout = TimeSpan.FromMinutes(5);
        return new HttpClientWrapper(client);
    }
}
