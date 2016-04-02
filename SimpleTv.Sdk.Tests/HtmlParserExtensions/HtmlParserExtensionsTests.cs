using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleTvSdk.Tests.Helpers;
using SimpleTv.Sdk.Http;
using HtmlAgilityPack;
using FluentAssertions;
using System.Linq;

namespace SimpleTv.Sdk.Tests
{
    [TestClass]
    public class HtmlParserExtensionsTests
    {
        [TestMethod]
        public void ParseMediaServers_ShouldParseMediaServers()
        {
            // Arrange
            var mainPage1 = SampleData.Get("HtmlParserExtensions.HtmlData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var mediaServers = html.ParseMediaServers(null);

            // Assert
            mediaServers.Count.Should().Be(1);
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseEpisode()
        {
            // Arrange
            var mainPage1 = SampleData.Get("HtmlParserExtensions.HtmlData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].EpisodeName.Should().Be("Some Title with a ;");
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseEpisode_WhenValuesAreMissing()
        {
            // Arrange
            var page = SampleData.Get("HtmlParserExtensions.HtmlData.MainPage2.html");
            var html = new HtmlDocument();
            html.LoadHtml(page);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes.Count.Should().Be(1);
            episodes[0].EpisodeName.Should().Be(null);
            episodes[0].Description.Should().Be(null);
            episodes[0].SeasonNumber.Should().Be(0);
            episodes[0].EpisodeNumber.Should().Be(0);
            episodes[0].Id.Should().BeEmpty();
            episodes[0].InstanceId.Should().BeEmpty();
        }

    }
}
