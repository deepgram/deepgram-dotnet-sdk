

namespace Deepgram.Tests.UnitTests.UtilitiesTests;
internal class UserAgentTest
{
    [Test]
    public void UserAgent_Should_Return_String_Value_Parameter()
    {
        //Act
        var result = UserAgentUtil.GetInfo();

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<string>());
        Assert.That(result, Does.Contain("deepgram"));
    }
}
