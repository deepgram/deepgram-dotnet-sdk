// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Exceptions.v1;

public class DeepgramRESTException : DeepgramException
{
    public DeepgramRESTException(string errMsg = "") : base(errMsg)
    {
    }
}
