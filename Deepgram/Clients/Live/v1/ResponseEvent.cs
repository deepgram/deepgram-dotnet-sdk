// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using WS = Deepgram.Clients.Listen.v1.WebSocket;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Live.v1;

public class ResponseEvent<T>(T? response) : WS.ResponseEvent<T>(response)
{
}
