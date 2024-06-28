// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.PreRecorded.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Analyze Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public interface IPreRecordedClient : IListenRESTClient
{
}
