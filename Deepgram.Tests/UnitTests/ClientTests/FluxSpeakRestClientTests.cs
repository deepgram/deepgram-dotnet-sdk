// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.REST;
using Deepgram.Clients.Flux.Speak.REST;
using Deepgram.Utilities;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class FluxSpeakRestClientTests
{
    DeepgramHttpClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey) { OnPrem = true };
    }

    #region URI construction
    [Test]
    public void GetUri_With_Default_BaseAddress_Should_Route_To_V2_Speak()
    {
        var uri = Client.GetUri(_options);

        uri.Should().Be("https://api.deepgram.com/v2/speak");
        uri.Should().NotContain("/v1");
    }

    [Test]
    public void GetUri_With_Custom_BaseAddress_Should_Route_To_V2_Speak()
    {
        var options = new DeepgramHttpClientOptions(_apiKey, "http://localhost:8080") { OnPrem = true };

        var uri = Client.GetUri(options);

        // DeepgramHttpClientOptions appends the default /v1; the batch client must strip it.
        uri.Should().Be("http://localhost:8080/v2/speak");
    }

    [Test]
    public void GetUri_With_Versioned_BaseAddress_Should_Replace_Version()
    {
        var options = new DeepgramHttpClientOptions(_apiKey, "https://example.com/v3") { OnPrem = true };

        var uri = Client.GetUri(options);

        uri.Should().Be("https://example.com/v2/speak");
        uri.Should().NotContain("/v3");
    }
    #endregion

    #region Request shape (URL + query params + body)
    [Test]
    public void Request_Url_Should_Pin_Path_And_Query_Param_Names()
    {
        var schema = new SpeakSchema
        {
            Model = "flux-alexis-en",
            Encoding = "mp3",
            Container = "none",
            SampleRate = 24000,
            BitRate = 48000,
            MipOptOut = false,
            Tag = new List<string> { "prod" },
            Priority = "low",
        };

        var url = QueryParameterUtil.FormatURL(Client.GetUri(_options), schema);

        using (new AssertionScope())
        {
            url.Should().StartWith("https://api.deepgram.com/v2/speak?");
            url.Should().Contain("model=flux-alexis-en");
            url.Should().Contain("encoding=mp3");
            url.Should().Contain("container=none");
            url.Should().Contain("sample_rate=24000");
            url.Should().Contain("bit_rate=48000");
            url.Should().Contain("mip_opt_out=false");
            url.Should().Contain("tag=prod");
            url.Should().Contain("priority=low");
        }
    }

    [Test]
    public void Request_Url_Should_Serialize_Integer_SampleRate_And_BitRate_Without_Decimal()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var schema = new SpeakSchema { Model = "flux-alexis-en", Encoding = "linear16", SampleRate = 44100, BitRate = 48000 };

            var url = QueryParameterUtil.FormatURL(Client.GetUri(_options), schema);

            using (new AssertionScope())
            {
                url.Should().Contain("sample_rate=44100");
                url.Should().Contain("bit_rate=48000");
                url.Should().NotContain("44100.");
                url.Should().NotContain("44.100");
                url.Should().NotContain("44,100");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void Request_Url_Should_Repeat_Tag_Parameters()
    {
        var schema = new SpeakSchema { Model = "flux-alexis-en", Tag = new List<string> { "a", "b" } };

        var url = QueryParameterUtil.FormatURL(Client.GetUri(_options), schema);

        url.Split('&').Count(p => p.Contains("tag=")).Should().Be(2);
    }

    [Test]
    public void TextSource_Should_Serialize_To_Text_Body()
    {
        var source = new TextSource("Your appointment is confirmed for 3pm tomorrow.");

        using var doc = JsonDocument.Parse(source.ToString());

        doc.RootElement.GetProperty("text").GetString().Should().Be("Your appointment is confirmed for 3pm tomorrow.");
        doc.RootElement.EnumerateObject().Count().Should().Be(1);
    }
    #endregion

    #region Dual response mode dispatch
    [Test]
    public void ToStream_With_CallBack_In_Schema_Should_Throw()
    {
        // ToStream is the synchronous/binary path; supplying a callback belongs to StreamCallBack.
        var client = new FluxSpeakRESTClient(_apiKey, _options);
        var schema = new SpeakSchema { Model = "flux-alexis-en", CallBack = "https://example.com/hook" };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.ToStream(new TextSource("hi"), schema));
    }

    [Test]
    public void StreamCallBack_With_No_CallBack_Anywhere_Should_Throw()
    {
        var client = new FluxSpeakRESTClient(_apiKey, _options);
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.StreamCallBack(new TextSource("hi"), null, schema));
    }

    [Test]
    public void StreamCallBack_With_CallBack_In_Both_Places_Should_Throw()
    {
        var client = new FluxSpeakRESTClient(_apiKey, _options);
        var schema = new SpeakSchema { Model = "flux-alexis-en", CallBack = "https://example.com/a" };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.StreamCallBack(new TextSource("hi"), "https://example.com/b", schema));
    }

    [Test]
    public void AsyncResponse_Should_Deserialize_CallBack_Ack()
    {
        var response = JsonSerializer.Deserialize<AsyncResponse>(
            """{ "request_id": "550e8400-e29b-41d4-a716-446655440000" }""");

        response!.RequestId.Should().Be("550e8400-e29b-41d4-a716-446655440000");
    }

    [Test]
    public void SyncResponse_Should_Deserialize_And_RoundTrip()
    {
        var response = new SyncResponse
        {
            ContentType = "audio/mpeg",
            RequestId = "r",
            ModelName = "flux-alexis-en",
            Characters = 47,
        };

        var roundTrip = JsonSerializer.Deserialize<SyncResponse>(response.ToString());

        using (new AssertionScope())
        {
            roundTrip!.ContentType.Should().Be("audio/mpeg");
            roundTrip.ModelName.Should().Be("flux-alexis-en");
            roundTrip.Characters.Should().Be(47);
        }
    }
    #endregion

    #region EA surface guarantees & factory
    [Test]
    public void FluxSpeak_Rest_Schema_Should_Not_Expose_Speed()
    {
        typeof(SpeakSchema).GetProperty("Speed").Should().BeNull();
    }

    [Test]
    public void ClientFactory_Should_Create_FluxSpeakRESTClient()
    {
        var client = ClientFactory.CreateFluxSpeakRESTClient(_apiKey, _options);

        using (new AssertionScope())
        {
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<Deepgram.Clients.Interfaces.v2.IFluxSpeakRESTClient>();
            client.Should().BeOfType<FluxSpeakRESTClient>();
        }
    }

    [Test]
    public void Options_Should_Carry_ApiKey_And_AccessToken_For_Rest()
    {
        using (new AssertionScope())
        {
            var keyed = new DeepgramHttpClientOptions("my-api-key") { OnPrem = true };
            keyed.ApiKey.Should().Be("my-api-key");

            var tokened = new DeepgramHttpClientOptions(null, accessToken: "my-access-token") { OnPrem = true };
            tokened.AccessToken.Should().Be("my-access-token");

            new FluxSpeakRESTClient("my-api-key", keyed).Should().NotBeNull();
            new FluxSpeakRESTClient("", tokened).Should().NotBeNull();
        }
    }
    #endregion
}
