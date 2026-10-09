// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram;

public static class ListenWebSocketClientExtensions
{
    /// <summary>
    /// Sends a Configure message on an open <c>/v1/listen</c> stream without reconnecting.
    /// Keyterms require a Nova-3 model and the global endpoint. A successful Configure message
    /// does not receive an acknowledgement; server rejections arrive as an Error response.
    /// </summary>
    public static async Task SendConfigure(this IListenWebSocketClient client, ConfigureSchema configure)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        // Configure applies at an audio boundary. Drain preceding queued audio before sending its
        // immediate control frame so sequential Send(...) then SendConfigure(...) calls preserve order.
        await client.Flush();
        await client.SendMessageImmediately(Encoding.UTF8.GetBytes(configure.ToString()));
    }
}
