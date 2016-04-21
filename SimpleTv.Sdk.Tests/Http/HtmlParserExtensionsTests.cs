using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleTvSdk.Tests.Helpers;
using SimpleTv.Sdk.Http;
using HtmlAgilityPack;
using FluentAssertions;
using SimpleTv.Sdk.Diagnostics;
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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

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
            var episodes = html.ParseEpisodes(null).ToList();

            // Assert
            episodes[0].ChannelNumber.Should().Be("15.1");
        }

        [TestMethod]
        public void ParseEpisodes_ShouldParseError()
        {
            // Arrange
            var mainPage1 = SampleData.Get("Http.TestData.ShowDetail2.html");
            var html = new HtmlDocument();
            html.LoadHtml(mainPage1);

            // Act
            var episodes = html.ParseEpisodes(null).ToList();

            // Assert
            episodes.Count.Should().Be(2);
            episodes[0].Error.Should().Be("This program could not be recorded. Simple.TV was unable to tune to the selected channel.");
            episodes[1].Error.Should().Be("This program could not be recorded. Simple.TV was unable to tune to the selected channel.");
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
            var episodes = html.ParseEpisodes(null).ToList();

            // Assert
            episodes[0].DateTime.Should().Be(null);
        }

        [TestMethod]
        public void ParseEpisodeLocation_ShouldThrowExceptionWhenDenyStreamingTrue()
        {
            // Arrange
            var page = SampleData.Get("Http.TestData.EpisodePlayer1.html");
            var html = new HtmlDocument();
            html.LoadHtml(page);

            // Act
            Action act = () => html.ParseEpisodeLocation();

            // Assert
            act.ShouldThrow<DenyStreamingException>().WithMessage("Please upgrade to the Simple.TV Premier Service to download this show remotely.");
        }

        [TestMethod]
        public void ParseEpisodeLocation_ShouldParseEpisodeLocation()
        {
            // Arrange
            var page = SampleData.Get("Http.TestData.EpisodePlayer2.html");
            var html = new HtmlDocument();
            html.LoadHtml(page);

            // Act
            var episodeLocation = html.ParseEpisodeLocation();

            // Assert
            episodeLocation.Should().Be("/7fa7fa16-9e45/tv.4500000.100");
        }

        [TestMethod]
        public void ParseEpisodes_ShouldParseIdsWhenNotInFirstAnchorTag()
        {
            // Arrange
            var page = SampleData.Get("Http.TestData.ShowDetail1.html");
            var html = new HtmlDocument();
            html.LoadHtml(page);

            // Act
            var episodes = html.ParseEpisodes(new Models.Show()).ToList();

            // Assert
            episodes.Count.Should().Be(1);
            episodes[0].Id.Should().Be(new Guid("8531ba48-b88f-11e3-9541-22000aa62ab4"));
            episodes[0].InstanceId.Should().Be(new Guid("54e4e8e6-e6f1-11e3-ae60-22000b278f17"));
        }
    }
}
