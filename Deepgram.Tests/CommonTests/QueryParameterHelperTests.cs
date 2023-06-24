using System;
using Deepgram.Models;
using Xunit;

namespace Deepgram.Tests.CommonTests
{
    public class QueryParameterHelperTests
    {
        [Fact]
        public void GetParameters_Should_GetParameters_Should_Return_String_When_Passing_String_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new LiveTranscriptionOptions()
            {
                Model = "Model"
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Model).ToLower()}={obj.Model.ToLower()}", result);
        }
        [Fact]
        public void GetParameters_Should_GetParameters_Should_Return_String_When_Passing_Int_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new LiveTranscriptionOptions()
            {
                Channels = 1,
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Channels).ToLower()}={obj.Channels}", result);
        }

        [Fact]
        public void GetParameters_Should_GetParameters_Should_Return_String_When_Passing_Array_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new LiveTranscriptionOptions()
            {
                Keywords = new[] { "key", "word" }
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Keywords).ToLower()}={obj.Keywords[0].ToLower()}", result);
            Assert.Contains($"{nameof(obj.Keywords).ToLower()}={obj.Keywords[1].ToLower()}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Decimal_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new PrerecordedTranscriptionOptions()
            {
                UtteranceSplit = (decimal)2.223
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"utt_split={obj.UtteranceSplit}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Boolean_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new PrerecordedTranscriptionOptions()
            {
                Paragraphs = true
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Paragraphs).ToLower()}={obj.Paragraphs.ToString()?.ToLower()}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_DateTime_Parameter()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed

            var obj = new DateTimeObject()
            {
                Time = new DateTime(2023, 5, 23)
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Time).ToLower()}=2023-05-23", result);
        }

        [Fact]
        public void GetParameters_Should_Return_Empty_String_When_Parameter_Has_No_Values()
        {
            //Arrange 
            //only creating a limited object so to test each value is being processed
            var obj = new LiveTranscriptionOptions()
            {
                Version = null
            };

            //Act
            var result = Helpers.QueryParameterHelper.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);

        }

        public class DateTimeObject
        {
            public DateTime Time { get; set; }
        }

    }
}