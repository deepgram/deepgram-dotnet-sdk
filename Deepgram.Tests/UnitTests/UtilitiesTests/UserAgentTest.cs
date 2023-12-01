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
