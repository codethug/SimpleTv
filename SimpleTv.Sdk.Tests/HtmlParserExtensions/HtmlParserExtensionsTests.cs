using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleTvSdk.Tests.Helpers;
using SimpleTv.Sdk.Http;
using HtmlAgilityPack;
using FluentAssertions;

namespace SimpleTv.Sdk.Tests
{
    [TestClass]
    public class HtmlParserExtensionsTests
    {
        [TestMethod]
        public void CanParseMediaServersFromMainPage()
        {
            // Arrange
            var mainPage1 = SampleData.Get("HtmlParsing.HtmlData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var mediaServers = html.ParseMediaServers(null);

            // Assert
            mediaServers.Count.Should().Be(1);
        }
    }
}
