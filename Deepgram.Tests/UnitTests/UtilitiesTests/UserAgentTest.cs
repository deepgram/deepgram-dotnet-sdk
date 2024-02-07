// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Tests.UnitTests.UtilitiesTests;
internal class UserAgentTest
{
    [Test]
    public void UserAgent_Should_Return_String_Value_Parameter()
    {
        //Act
        var result = UserAgentUtil.GetInfo();

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<string>();
            result.Should().Contain("deepgram/");
            result.Should().Contain("dotnet/");
        }
    }
}
