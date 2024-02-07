// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Extensions;
public static class ClientWebSocketExtensions
{
    public static ClientWebSocket SetHeaders(this ClientWebSocket clientWebSocket, string apiKey, DeepgramClientOptions? options)
    {
        clientWebSocket.Options.SetRequestHeader("Authorization", $"token {apiKey}");

        if (options is not null && options.Headers is not null)
            foreach (var header in options.Headers)
            { clientWebSocket.Options.SetRequestHeader(header.Key, header.Value); }

        return clientWebSocket;
    }
}
