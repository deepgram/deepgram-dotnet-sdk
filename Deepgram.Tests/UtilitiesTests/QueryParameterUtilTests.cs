using Deepgram.Models;
using Deepgram.Tests.Fakes;
using Deepgram.Utilities;
using Xunit;

namespace Deepgram.Tests.UtilitiesTests
{
    public class QueryParameterUtilTests
    {
        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_String_Parameter()
        {
            //Arrange 
            var obj = FakeModels.PrerecordedTranscriptionOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Model).ToLower()}={obj.Model.ToLower()}", result);
        }
        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Int_Parameter()
        {
            //Arrange 
            var obj = FakeModels.ListAllRequestsOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Limit).ToLower()}={obj.Limit}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Array_Parameter()
        {
            //Arrange 
            var obj = FakeModels.PrerecordedTranscriptionOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Keywords).ToLower()}={obj.Keywords[0].ToLower()}", result);

        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Decimal_Parameter()
        {
            //Arrange 
            var obj = FakeModels.PrerecordedTranscriptionOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"utt_split={obj.UtteranceSplit}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_Boolean_Parameter()
        {
            //Arrange 
            var obj = FakeModels.PrerecordedTranscriptionOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"{nameof(obj.Paragraphs).ToLower()}={obj.Paragraphs.ToString()?.ToLower()}", result);
        }

        [Fact]
        public void GetParameters_Should_Return_String_When_Passing_DateTime_Parameter()
        {
            //Arrange 
            var obj = FakeModels.ListAllRequestsOptions;

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Contains($"start=2023-05-23", result);
        }

        [Fact]
        public void GetParameters_Should_Return_Empty_String_When_Parameter_Has_No_Values()
        {
            //Arrange 
            var obj = new LiveTranscriptionOptions();

            //Act
            var result = QueryParameterUtil.GetParameters(obj);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }
    }
}