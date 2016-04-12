using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleTv.Sdk.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Tests.Http
{
    [TestClass]
    public class HtmlDocumentClientTests
    {
        [TestMethod]
        public void GetRaw_ShouldFireHttpResponseReceived()
        {
            // Arrange
            var uri = new Uri("http://a.com");
            var mockWebClient = new Mock<IWebClient>();
            mockWebClient.Setup(wc => wc.DownloadString(uri)).Returns(String.Empty);
            var client = new HtmlDocumentClient(mockWebClient.Object);

            bool httpResponseReceived_Fired = false;
            client.HttpResponseReceived += (s, e) => httpResponseReceived_Fired = true;

            // Act
            client.GetRaw(new Uri("http://a.com"));

            // Assert
            httpResponseReceived_Fired.Should().BeTrue();
        }

        [TestMethod]
        public void GetDocument_ShouldFireHttpResponseReceived()
        {
            // Arrange
            var uri = new Uri("http://a.com");
            var mockWebClient = new Mock<IWebClient>();
            mockWebClient.Setup(wc => wc.DownloadString(uri)).Returns(String.Empty);
            var client = new HtmlDocumentClient(mockWebClient.Object);

            bool httpResponseReceived_Fired = false;
            client.HttpResponseReceived += (s, e) => httpResponseReceived_Fired = true;

            // Act
            client.GetDocument(new Uri("http://a.com"));

            // Assert
            httpResponseReceived_Fired.Should().BeTrue();
        }

        [TestMethod]
        public void PostRawReponse_ShouldFireHttpResponseReceived()
        {
            // Arrange
            var uri = new Uri("http://a.com");
            var mockWebClient = new Mock<IWebClient>();
            mockWebClient.Setup(wc => wc.Headers).Returns(new WebHeaderCollection());
            mockWebClient.Setup(wc => wc.UploadString(uri, It.IsAny<string>())).Returns(String.Empty);
            var client = new HtmlDocumentClient(mockWebClient.Object);

            bool httpResponseReceived_Fired = false;
            client.HttpResponseReceived += (s, e) => httpResponseReceived_Fired = true;

            // Act
            client.PostRawReponse(new Uri("http://a.com"), "description", null);

            // Assert
            httpResponseReceived_Fired.Should().BeTrue();
        }

        [TestMethod]
        public void FormatQueryString_ShouldConcatenateNamesAndValuesAsQueryString()
        {
            // Arrange
            var data = new NameValueCollection {
                { "Foo", "abc" },
                { "Bar", "def" }
            };
            var client = new HtmlDocumentClient(null);

            // Act
            var result = client.FormatQueryString(data);

            // Assert
            result.Should().Be("Foo=abc&Bar=def");
        }

        [TestMethod]
        public void FormatQueryString_ShouldReturnEmptyStringIfInputNull()
        {
            // Arrange
            NameValueCollection data = null;
            var client = new HtmlDocumentClient(null);

            // Act
            var result = client.FormatQueryString(data);

            // Assert
            result.Should().Be(String.Empty);
        }

    }
}
