// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.UnitTests.LoggerProviderTests;
public class LogProviderTest
{
    [Test]
    public void SetLogFactory_Should_Set_LoggerFactory()
    {
        //Arrange 
        var loggerFactory = Substitute.For<ILoggerFactory>();

        //Act
        LogProvider.SetLogFactory(loggerFactory);

        //Assert 
        LogProvider._loggerFactory.Should().NotBeNull();
    }
}
