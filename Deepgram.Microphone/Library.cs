// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Microphone;

public class Library
{
    public static void Initialize()
    {
        PortAudio.Initialize();
    }

    public static void Terminate()
    {
        // Terminate PortAudio
        PortAudio.Terminate();
    }
}
