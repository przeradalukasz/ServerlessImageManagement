using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using RecognitionOrderValidator;
using FunctionTestHelper;
using Microsoft.Extensions.Primitives;
using ServerlessImageManagement;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ServerlessTests
{
    public class RecognitionStartTests : FunctionTest
    {
        [Fact]
        public async void Function_Returns_Bad_Request_When_Order_Not_Valid()
        {
            var order = new RecognitionOrder();
            var queueCollector = new AsyncCollector<RecognitionOrder>();
            var mockValidator = new Mock<IRecOrderValidator>();
            mockValidator.Setup(x => x.IsValid(It.IsAny<RecognitionOrder>())).Returns(false);

            var query = new Dictionary<String, StringValues>();
            var body = JsonConvert.SerializeObject(order);

            var result = await RecognitionStart.Run(req: HttpRequestSetup(query, body), validator: mockValidator.Object, queueWithRecOrders: queueCollector, log: log);
            var resultObject = (BadRequestObjectResult)result;
            Assert.Equal("Provided data is invalid", resultObject.Value);
        }

        [Fact]
        public async void Function_Returns_Ok_Result_And_Adds_Message_To_Collector_When_Order_Is_Valid()
        {
            var order = new RecognitionOrder()
            {
                DestinationFolder = "testFolder",
                EmailAddress = "test@gmail.com",
                PhoneNumber = "+123456789",
                PhotosSource = "testSource",
                RecognitionName = "testName",
                PatternFaces = new string[] { }
            };

            var queueCollector = new AsyncCollector<RecognitionOrder>();
            var mockValidator = new Mock<IRecOrderValidator>();
            mockValidator.Setup(x => x.IsValid(It.IsAny<RecognitionOrder>())).Returns(true);

            var query = new Dictionary<String, StringValues>();
            var body = JsonConvert.SerializeObject(order);

            var result = await RecognitionStart.Run(req: HttpRequestSetup(query, body), validator: mockValidator.Object, queueWithRecOrders: queueCollector, log: log);
            
            Assert.IsType<OkResult>(result);
            Assert.NotEmpty(queueCollector.Items);
            Assert.Equal(order.DestinationFolder, queueCollector.Items[0].DestinationFolder);
            Assert.Equal(order.EmailAddress, queueCollector.Items[0].EmailAddress);
            Assert.Equal(order.PhoneNumber, queueCollector.Items[0].PhoneNumber);
            Assert.Equal(order.PhotosSource, queueCollector.Items[0].PhotosSource);
            Assert.Equal(order.RecognitionName, queueCollector.Items[0].RecognitionName);
            Assert.Equal(order.PatternFaces, queueCollector.Items[0].PatternFaces);
        }
    }
}
