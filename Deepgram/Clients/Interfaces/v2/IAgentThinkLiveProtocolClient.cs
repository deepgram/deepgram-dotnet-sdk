// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// Adds Think Live event subscriptions to the current Voice Agent protocol client.
/// </summary>
public interface IAgentThinkLiveProtocolClient : IAgentProtocolClient
{
    Task<bool> Subscribe(EventHandler<ThinkLiveUpdatedResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<FrustrationDetectedResponse> eventHandler);
}
