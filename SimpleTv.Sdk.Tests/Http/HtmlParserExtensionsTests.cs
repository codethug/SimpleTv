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
        public void ParseMediaServers_ShouldParseMediaServers()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var mediaServers = html.ParseMediaServers(null);

            // Assert
            mediaServers.Count.Should().Be(1);
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseName()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
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
            var page = SampleData.Get("Http.TestData.MainPage2.html");
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

        [TestMethod]
        public void ParseEpisode_ShouldParseDescription()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].Description.Should().Be("In this great episode, amazing things happen");
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseSeasonNumber()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].SeasonNumber.Should().Be(1);
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseEpisodeNumber()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].EpisodeNumber.Should().Be(3);
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseEpisodeId()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].Id.Should().Be(new Guid("6bbf69eb-5944-11e5-b06f-22000b688027"));
        }

        [TestMethod]
        public void ParseEpisode_ShouldParseInstanceId()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].InstanceId.Should().Be(new Guid("20f69334-5948-11e5-b06f-22000b688027"));
        }

        [TestMethod]
        public void ParseEpisodes_ShouldParseDateTime()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].DateTime.Should().Be(24.September(2015).At(20,00));
        }

        [TestMethod]
        public void ParseEpisodes_ShouldParseChannelNumber()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage1.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].ChannelNumber.Should().Be("15.1");
        }

        [TestMethod]
        public void ParseShows_ShouldParseMultipleShows()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage3.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var shows = html.ParseShows(null, null);

            // Assert
            shows.Count.Should().Be(2);
        }

        [TestMethod]
        public void ParseShows_ShouldParseId()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage3.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var shows = html.ParseShows(null, null);

            // Assert
            shows[0].Id.Should().Be(new Guid("c868a1ab-468f-11e5-b06f-22000b688027"));
        }

        [TestMethod]
        public void ParseShows_ShouldParseName()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage3.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var shows = html.ParseShows(null, null);

            // Assert
            shows[0].Name.Should().Be("NCIS");
        }

        [TestMethod]
        public void ParseShows_ShouldParseNumEpisodes()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage3.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var shows = html.ParseShows(null, null);

            // Assert
            shows[0].NumEpisodes.Should().Be(5);
        }

        [TestMethod]
        public void ParseEpisodes_ShouldHaveNullDateTimeIfCantBeParsed()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.MainPage4.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null, null);

            // Assert
            episodes[0].DateTime.Should().Be(null);
        }
    }
}
