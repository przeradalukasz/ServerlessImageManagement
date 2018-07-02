using RecognitionOrderValidator;
using System.Collections.Generic;
using Xunit;

namespace ServerlessTests
{
    public class RecOrderValidatorTests
    {
        [Fact]
        public void IsValid_Should_Return_True_When_Order_Is_Valid()
        {
            var validator = new RecOrderValidator();

            var order = new RecognitionOrder()
            {
                DestinationFolder = "testFolder",
                EmailAddress = "test@gmail.com",
                PhoneNumber = "+123456789",
                PhotosSource = "testSource",
                RecognitionName = "testName",
                PatternFaces = new string[] { "testFace" }
            };

            Assert.True(validator.IsValid(order));
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Destination_Folder_Is_Empty()
        {
            var validator = new RecOrderValidator();

            var order = new RecognitionOrder()
            {
                DestinationFolder = "",
                EmailAddress = "test@gmail.com",
                PhoneNumber = "+123456789",
                PhotosSource = "testSource",
                RecognitionName = "testName",
                PatternFaces = new string[] { "testFace" }
            };

            Assert.False(validator.IsValid(order));
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Pattern_Faces_Array_Is_Empty()
        {
            var validator = new RecOrderValidator();

            var order = new RecognitionOrder()
            {
                DestinationFolder = "testFolder",
                EmailAddress = "test@gmail.com",
                PhoneNumber = "+123456789",
                PhotosSource = "testSource",
                RecognitionName = "testName",
                PatternFaces = new string[] {}
            };

            Assert.False(validator.IsValid(order));
        }
    }
}
