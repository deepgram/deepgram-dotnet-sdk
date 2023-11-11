namespace Deepgram.Tests.ClientTests;

[TestFixture]
public class AbstractRestfulClientTests
{
    ILogger<ConcreteRestClient> logger;

    [SetUp]
    public void Setup()
    {
        logger = Substitute.For<ILogger<ConcreteRestClient>>();
    }
}
