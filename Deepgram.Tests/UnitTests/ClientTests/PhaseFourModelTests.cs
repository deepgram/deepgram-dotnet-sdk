// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Listen.v1.REST;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.Speak.v1.REST;
using Deepgram.Utilities;
using ListenMetadata = Deepgram.Models.Listen.v1.REST.Metadata;
using ListenV1LiveSchema = Deepgram.Models.Listen.v1.WebSocket.LiveSchema;
using ListenV2LiveSchema = Deepgram.Models.Listen.v2.WebSocket.LiveSchema;
using SpeakV1WebSocketSchema = Deepgram.Models.Speak.v1.WebSocket.SpeakSchema;
using System.Globalization;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class PhaseFourModelTests
{
    [Test]
    public void Current_Request_Options_Should_Serialize_To_Their_Wire_Names()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var speak = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/speak", new SpeakSchema
            {
                Speed = 1.2,
                Tag = new List<string> { "release", "phase-four" },
                MipOptOut = true,
            });
            var listen = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/listen", new PreRecordedSchema { MipOptOut = true });
            var analyze = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/read", new AnalyzeSchema { Tag = new List<string> { "release", "phase-four" } });
            var listenV1 = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/listen", new ListenV1LiveSchema { MipOptOut = true });
            var listenV2 = QueryParameterUtil.FormatURL("https://api.deepgram.com/v2/listen", new ListenV2LiveSchema { MipOptOut = true });
            var speakWebSocket = QueryParameterUtil.FormatURL("https://api.deepgram.com/v1/speak", new SpeakV1WebSocketSchema { MipOptOut = true });

            using (new AssertionScope())
            {
                speak.Should().Contain("speed=1.2").And.Contain("tag=release").And.Contain("tag=phase-four").And.Contain("mip_opt_out=true");
                listen.Should().Contain("mip_opt_out=true");
                analyze.Should().Contain("tag=release").And.Contain("tag=phase-four");
                listenV1.Should().Contain("mip_opt_out=true");
                listenV2.Should().Contain("mip_opt_out=true");
                speakWebSocket.Should().Contain("mip_opt_out=true");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void Current_Response_Fields_Should_Deserialize()
    {
        var metadata = JsonSerializer.Deserialize<ListenMetadata>("""{"diarize_info":{"model_uuid":"model-uuid","arch":"nova"},"tags":["release","phase-four"]}""");
        var project = JsonSerializer.Deserialize<ProjectResponse>("""{"mip_opt_out":true}""");
        var request = JsonSerializer.Deserialize<UsageRequest>("""{"code":429,"deployment":"self-hosted"}""");
        var balance = JsonSerializer.Deserialize<BalanceResponse>("""{"purchase_order_id":"po-123","purchase":"legacy"}""");

        using (new AssertionScope())
        {
            metadata!.DiarizeInfo!.ModelUuid.Should().Be("model-uuid");
            metadata.DiarizeInfo.Arch.Should().Be("nova");
            metadata.Tags.Should().Equal("release", "phase-four");
            project!.MipOptOut.Should().BeTrue();
            request!.Code.Should().Be(429d);
            request.Deployment.Should().Be("self-hosted");
            balance!.PurchaseOrderId.Should().Be("po-123");
            balance.Purchase.Should().Be("legacy");
        }
    }
}
