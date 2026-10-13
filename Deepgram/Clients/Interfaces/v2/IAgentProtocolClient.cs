// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// Adds current Voice Agent protocol events without changing the legacy
/// <see cref="IAgentWebSocketClient"/> contract.
/// </summary>
public interface IAgentProtocolClient : IAgentWebSocketClient
{
    /// <summary>
    /// Flushes queued audio and sends ForceEndTurn after it reaches the socket.
    /// </summary>
    Task SendForceEndTurn();
    Task<bool> Subscribe(EventHandler<ListenUpdatedResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<ThinkUpdatedResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<FunctionCallCancelledResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<FunctionCallResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<CustomFromThinkProviderResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<LatencyReportResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<AgentWarningResponse> eventHandler);
    Task<bool> Subscribe(EventHandler<AgentHistoryResponse> eventHandler);
}
