// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Listen.v1.WebSocket;

namespace Deepgram;

public static class ListenWebSocketClientExtensions
{
    /// <summary>
    /// Sends a Configure message without reconnecting the Listen v1 stream.
    /// </summary>
    public static void SendConfigure(this IListenWebSocketClient client, ConfigureSchema configure)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        client.SendMessageImmediately(Encoding.UTF8.GetBytes(configure.ToString()));
    }
}
