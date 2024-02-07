// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Live.v1;

namespace Deepgram.DeepgramEventArgs;

public class ResponseReceivedEventArgs(EventResponse response) : EventArgs
{
    public EventResponse Response { get; set; } = response;
}

