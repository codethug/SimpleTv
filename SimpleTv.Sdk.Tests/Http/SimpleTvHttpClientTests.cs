using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NodaTime;
using SimpleTv.Sdk.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Tests.Http
{
    [TestClass]
    public class SimpleTvHttpClientTests
    {
        [TestMethod]
        public void IsBigEnoughToDownload_ShouldReturnTrueIfFileIsBigEnough()
        {
            // Arrange
            var clockMock = new Mock<IClock>();
            var tzProviderMock = new Mock<IDateTimeZoneProvider>();
            var webClientMock = new Mock<IWebClient>();
            webClientMock.Setup(wc => wc.GetFileSize(It.IsAny<Uri>())).Returns(5000);
            var docClientMock = new Mock<IHtmlDocumentClient>();
            var client = new SimpleTvHttpClient(clockMock.Object, tzProviderMock.Object, webClientMock.Object, docClientMock.Object);

            // Act
            var result = client.IsBigEnoughToDownload(null, 1000, "Great Episode Name");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsBigEnoughToDownload_ShouldReturnFalseIfFileIsNotBigEnough()
        {
            // Arrange
            var clockMock = new Mock<IClock>();
            var tzProviderMock = new Mock<IDateTimeZoneProvider>();
            var webClientMock = new Mock<IWebClient>();
            webClientMock.Setup(wc => wc.GetFileSize(It.IsAny<Uri>())).Returns(500);
            var docClientMock = new Mock<IHtmlDocumentClient>();
            var client = new SimpleTvHttpClient(clockMock.Object, tzProviderMock.Object, webClientMock.Object, docClientMock.Object);

            // Act
            var result = client.IsBigEnoughToDownload(null, 10000, "Great Episode Name");

            // Assert
            result.Should().BeFalse();
        }

    }
}
