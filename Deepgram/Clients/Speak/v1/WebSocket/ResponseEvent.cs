// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Speak.v1.WebSocket;

public class ResponseEvent<T> : EventArgs
{
    public T? Response { get; }

    public ResponseEvent(T? response)
    {
        Response = response;
    }
}

