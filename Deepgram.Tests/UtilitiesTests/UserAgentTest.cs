namespace Deepgram.Tests.UtilitiesTests;
internal class UserAgentTest
{
    [Test]
    public void UserAgent_Should_Return_String_Value_Parameter()
    {
        //Act
        var result = UserAgentUtil.GetInfo();

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.IsAssignableFrom<string>(result);
        StringAssert.StartsWith("deepgram", result);
    }
}
