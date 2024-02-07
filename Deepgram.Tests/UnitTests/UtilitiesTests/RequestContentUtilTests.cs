// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class RequestContentUtilTests
{
    [Test]
    public void CreatePayload_Should_Return_StringContent()
    {

        //Arrange       
        var project = new AutoFaker<Project>().Generate();

        //Act
        var result = RequestContentUtil.CreatePayload(project);

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
        //Arrange 
        var source = System.Text.Encoding.ASCII.GetBytes("Acme Unit Testing");
        var stream = new MemoryStream(source);

        //Act
        var result = RequestContentUtil.CreateStreamPayload(stream);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<HttpContent>();
        }
    }


}
