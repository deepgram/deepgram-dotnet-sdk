// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Tests.UnitTests.ModelTests;

public class DeepgramExceptionTests
{
    [Test]
    public void RESTException_Should_Surface_Management_Error_Body_In_Message()
    {
        // Management endpoints (projects, keys, agents, agent-variables) return errors shaped
        // {"category","message","details","request_id"}. Before the Message override, this
        // deserialized into an exception with an EMPTY Message, so a console app printed a
        // bare exception type with no explanation.
        var json = """
        {
            "category": "INSUFFICIENT_PERMISSIONS",
            "message": "Your account does not have the required scope to perform that action for this project.",
            "details": "Check that your account has the 'agent:write' scope for this project.",
            "request_id": "e8cbcdd7-4ce6-4429-9313-cc4797433e81"
        }
        """;

        var exception = JsonSerializer.Deserialize<DeepgramRESTException>(json);

        using (new AssertionScope())
        {
            exception!.Category.Should().Be("INSUFFICIENT_PERMISSIONS");
            exception.ErrorMessage.Should().Contain("required scope");
            exception.Details.Should().Contain("agent:write");
            exception.RequestId.Should().Be("e8cbcdd7-4ce6-4429-9313-cc4797433e81");
            exception.Message.Should().Contain("INSUFFICIENT_PERMISSIONS");
            exception.Message.Should().Contain("required scope");
            exception.Message.Should().Contain("agent:write");
            exception.Message.Should().Contain("e8cbcdd7-4ce6-4429-9313-cc4797433e81");
        }
    }

    [Test]
    public void RESTException_Should_Surface_Classic_Error_Body_In_Message()
    {
        // Transcription-style endpoints return {"err_code","err_msg","request_id"}.
        var json = """
        {
            "err_code": "INVALID_AUTH",
            "err_msg": "Invalid credentials.",
            "request_id": "req-123"
        }
        """;

        var exception = JsonSerializer.Deserialize<DeepgramRESTException>(json);

        using (new AssertionScope())
        {
            exception!.Message.Should().Contain("INVALID_AUTH");
            exception.Message.Should().Contain("Invalid credentials.");
            exception.Message.Should().Contain("req-123");
        }
    }

    [Test]
    public void RESTException_Should_Tolerate_Empty_Error_Body()
    {
        var exception = JsonSerializer.Deserialize<DeepgramRESTException>("{}");

        // No useful body: Message falls back to the base exception message rather than
        // throwing or returning the "Unknown ..." sentinels as if they were real content.
        exception!.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void Exception_Constructor_Message_Should_Be_Unchanged()
    {
        // Back-compat: exceptions thrown with an explicit message keep exactly that message.
        new DeepgramException("boom").Message.Should().Be("boom");
        new DeepgramRESTException("boom").Message.Should().Be("boom");
    }
}
