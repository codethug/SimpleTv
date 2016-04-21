using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NodaTime;
using SimpleTv.Sdk.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Tests.Http
{
    [TestClass]
    public class SimpleTvHttpClientTests
    {
        [TestMethod]
        public void Login_ShouldReturnTrueIfLoginSucceeded()
        {
            // Arrange
            var clockMock = new Mock<IClock>();
            var tzProviderMock = new Mock<IDateTimeZoneProvider>();
            var webClientMock = new Mock<IWebClient>();
            var docClientMock = new Mock<IHtmlDocumentClient>();
            docClientMock.Setup(dc => dc.PostRawReponse(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .Returns("{}"); // No Error
            var client = new SimpleTvHttpClient(clockMock.Object, tzProviderMock.Object, webClientMock.Object, docClientMock.Object);

            // Act
            var result = client.Login("username", "password");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Login_ShouldReturnFalseIfLoginFails()
        {
            // Arrange
            var clockMock = new Mock<IClock>();
            var tzProviderMock = new Mock<IDateTimeZoneProvider>();
            var webClientMock = new Mock<IWebClient>();
            var docClientMock = new Mock<IHtmlDocumentClient>();
            docClientMock.Setup(dc => dc.PostRawReponse(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .Returns("{\"SignInError\":\"Error Logging In\"}");
            var client = new SimpleTvHttpClient(clockMock.Object, tzProviderMock.Object, webClientMock.Object, docClientMock.Object);

            // Act
            var result = client.Login("username", "password");

            // Assert
            result.Should().BeFalse();
        }
    }
}
