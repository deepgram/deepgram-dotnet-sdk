// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(string apiKey, DeepgramClientOptions? deepgramClientOptions)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
}
