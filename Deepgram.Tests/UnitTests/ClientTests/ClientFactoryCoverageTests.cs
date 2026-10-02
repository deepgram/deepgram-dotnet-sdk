// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using FluentAssertions;
using FluentAssertions.Execution;
using V1 = Deepgram.Clients.Interfaces.v1;
using V2 = Deepgram.Clients.Interfaces.v2;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ClientFactoryCoverageTests
{
    private const string ApiKey = "test-api-key";

    private static readonly DeepgramHttpClientOptions HttpOptions = new(ApiKey) { OnPrem = true };
    private static readonly DeepgramWsClientOptions WebSocketOptions = new(ApiKey) { OnPrem = true };

    [Test]
    public void CreateCurrentClients_Should_Return_Their_Published_Interfaces()
    {
        using (new AssertionScope())
        {
            ClientFactory.CreateAgentWebSocketClient(ApiKey, WebSocketOptions).Should().BeAssignableTo<V2.IAgentWebSocketClient>();
            ClientFactory.CreateAgentManageClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.IAgentManageClient>();
            ClientFactory.CreateAnalyzeClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.IAnalyzeClient>();
            ClientFactory.CreateAuthClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.IAuthClient>();
            ClientFactory.CreateFluxWebSocketClient(ApiKey, WebSocketOptions).Should().BeAssignableTo<V2.IFluxWebSocketClient>();
            ClientFactory.CreateFluxSpeakWebSocketClient(ApiKey, WebSocketOptions).Should().BeAssignableTo<V2.IFluxSpeakWebSocketClient>();
            ClientFactory.CreateFluxSpeakRESTClient(ApiKey, HttpOptions).Should().BeAssignableTo<V2.IFluxSpeakRESTClient>();
            ClientFactory.CreateListenWebSocketClient(ApiKey, WebSocketOptions).Should().BeAssignableTo<V2.IListenWebSocketClient>();
            ClientFactory.CreateListenRESTClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.IListenRESTClient>();
            ClientFactory.CreateManageClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.IManageClient>();
            ClientFactory.CreateSelfHostedClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.ISelfHostedClient>();
            ClientFactory.CreateSpeakRESTClient(ApiKey, HttpOptions).Should().BeAssignableTo<V1.ISpeakRESTClient>();
            ClientFactory.CreateSpeakWebSocketClient(ApiKey, WebSocketOptions).Should().BeAssignableTo<V2.ISpeakWebSocketClient>();
        }
    }

    [Test]
    public void CreateListenWebSocketClient_With_The_Latest_Version_Should_Return_That_Version()
    {
        ClientFactory.CreateListenWebSocketClient(2, ApiKey, WebSocketOptions)
            .Should().BeOfType<Deepgram.Clients.Listen.v2.WebSocket.Client>();
    }

    [Test]
    public void CreateListenWebSocketClient_With_An_Unsupported_Version_Should_Throw()
    {
        Action act = () => ClientFactory.CreateListenWebSocketClient(0, ApiKey, WebSocketOptions);

        act.Should().Throw<ArgumentException>().WithParameterName("version");
    }
}
