// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Exceptions.v1;

public class DeepgramRESTException : DeepgramException
{
    // Parameterless constructor used by JSON deserialization when an API error body is
    // rehydrated into this exception; the error content lands in the base-class properties
    // (Category/ErrorMessage/Details or ErrCode/ErrMsg, plus RequestId).
    // base(string.Empty) preserves the historical behavior of `new DeepgramRESTException()`,
    // which bound to the optional errMsg = "" constructor and produced an empty Message —
    // base() would instead surface the runtime-generated "Exception of type ..." text.
    public DeepgramRESTException() : base(string.Empty)
    {
    }

    public DeepgramRESTException(string errMsg) : base(errMsg)
    {
    }
}
