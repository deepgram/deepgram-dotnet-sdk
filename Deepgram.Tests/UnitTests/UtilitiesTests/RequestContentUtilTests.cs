// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Tests.UnitTests.UtilitiesTests;

public class HttpRequestUtilTests
{
    [Test]
    public void CreatePayload_Should_Return_StringContent()
    {

        // Input and Output       
        var project = new AutoFaker<Project>().Generate();

        //Act
        var result = HttpRequestUtil.CreatePayload(project);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<StringContent>();
        }
    }


    [Test]
    public void CreateStreamPayload_Should_Return_HttpContent()
    {
        // Input and Output 
        var source = System.Text.Encoding.ASCII.GetBytes("Acme Unit Testing");
        var stream = new MemoryStream(source);

        //Act
        var result = HttpRequestUtil.CreateStreamPayload(stream);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<HttpContent>();
        }
    }


}
