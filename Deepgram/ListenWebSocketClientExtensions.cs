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
    /// Keyterm updates require a Nova-3 model on <c>api.deepgram.com</c>; see
    /// <see cref="ConfigureSchema"/> for the keyterm limit and regional hosts. Most rejections
    /// arrive as an Error response. <c>SendConfigure</c> sends nothing while the client is not
    /// connected, including while Connect() is still in progress. Each new connection starts from
    /// its <see cref="LiveSchema"/> settings, so send the update again after every <c>Connect()</c>.
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
