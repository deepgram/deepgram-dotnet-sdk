
using Deepgram.Logger;
using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.UnitTests.LoggerProviderTests;
public class LogProviderTest
{
    [Test]
    public async Task SetLogFactory_Should_Set_LoggerFactory()
    {
        //Arrange 
        var loggerFactory = Substitute.For<ILoggerFactory>();

        //Act
        LogProvider.SetLogFactory(loggerFactory);

        //Assert 
        LogProvider._loggerFactory.Should().NotBeNull();
    }
}
