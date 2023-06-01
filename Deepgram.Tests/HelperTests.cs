using Deepgram.Common;
using Deepgram.Models;

namespace Deepgram.Tests;
public class HelperTests
{
    [Fact]
    public void Return_String_When_Passing_String_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new LiveTranscriptionOptions()
        {
            Model = "Model"
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"{nameof(obj.Model).ToLower()}={obj.Model.ToLower()}", SUT);
    }
    [Fact]
    public void Return_String_When_Passing_Int_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new LiveTranscriptionOptions()
        {
            Channels = 1,
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"{nameof(obj.Channels).ToLower()}={obj.Channels}", SUT);
    }

    [Fact]
    public void Return_String_When_Passing_Array_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new LiveTranscriptionOptions()
        {
            Keywords = new[] { "key", "word" }
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"{nameof(obj.Keywords).ToLower()}={obj.Keywords[0].ToLower()}", SUT);
        Assert.Contains($"{nameof(obj.Keywords).ToLower()}={obj.Keywords[1].ToLower()}", SUT);
    }

    [Fact]
    public void Return_String_When_Passing_Decimal_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new PrerecordedTranscriptionOptions()
        {
            UtteranceSplit = (decimal)2.223
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"utt_split={obj.UtteranceSplit}", SUT);
    }

    [Fact]
    public void Return_String_When_Passing_Boolean_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new PrerecordedTranscriptionOptions()
        {
            Paragraphs = true
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"{nameof(obj.Paragraphs).ToLower()}={obj.Paragraphs.ToString()?.ToLower()}", SUT);
    }

    [Fact]
    public void Return_String_When_Passing_DateTime_Parameter_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed

        var obj = new DateTimeObject()
        {
            Time = new DateTime(2023, 5, 23)
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Contains($"{nameof(obj.Time).ToLower()}=2023-05-23", SUT);
    }

    [Fact]
    public void Return_Empty_String_When_Parameter_Has_No_Values_Success()
    {
        //Arrange 
        //only creating a limited object so to test each value is being processed
        var obj = new LiveTranscriptionOptions()
        {
            Version = null
        };

        //Act
        var SUT = Helpers.GetParameters(obj);

        //Assert
        Assert.NotNull(SUT);
        Assert.Equal(string.Empty, SUT);

    }

    public class DateTimeObject
    {
        public DateTime Time { get; set; }
    }

}
