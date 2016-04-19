using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleTv.Sdk.Http;
using SimpleTv.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Tests
{
    [TestClass]
    public class SimpleTvClientTests
    {
        [TestMethod]
        public void GetShows_ShouldIncludeSpecifiedShowsWithWildcard()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetShows(It.IsAny<MediaServer>())).Returns(new Show[] {
                new Show() { Name = "Friends" },
                new Show() { Name = "Days of Our Lives" },
                new Show() { Name = "Friday Nights" }
            }.ToList());
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "Fri*";
            var excludeFilter = String.Empty; // exclude nothing

            // Act
            var result = tvClient.GetShows(new MediaServer(), includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(2);
            result[0].Name.Should().Be("Friends");
            result[1].Name.Should().Be("Friday Nights");
        }

        [TestMethod]
        public void GetShows_ShouldExcludeSpecifiedShows()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetShows(It.IsAny<MediaServer>())).Returns(new Show[] {
                new Show() { Name = "Friends" },
                new Show() { Name = "Days of Our Lives" },
                new Show() { Name = "Friday Nights" }
            }.ToList());
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "*"; // include everything
            var excludeFilter = "Fri*";

            // Act
            var result = tvClient.GetShows(new MediaServer(), includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(1);
            result[0].Name.Should().Be("Days of Our Lives");
        }

        [TestMethod]
        public void GetShows_ShouldHaveExcludeOverrideInclude()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetShows(It.IsAny<MediaServer>())).Returns(new Show[] {
                new Show() { Name = "Friends" },
                new Show() { Name = "Days of Our Lives" },
                new Show() { Name = "Friday Nights" }
            }.ToList());
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "Fri*";
            var excludeFilter = "Friends";

            // Act
            var result = tvClient.GetShows(new MediaServer(), includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(1);
            result[0].Name.Should().Be("Friday Nights");
        }

        [TestMethod]
        public void GetMediaServers_ShouldIncludeSpecifiedShows()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetMediaServers()).Returns(new MediaServer[] {
                new MediaServer() { Name = "Family Room" },
                new MediaServer() { Name = "Master Bedroom" },
                new MediaServer() { Name = "Basement" }
            }.ToList());
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(It.IsAny<MediaServer>())).Returns(true);
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "*oom";
            var excludeFilter = String.Empty; // Exclude nothing

            // Act
            var result = tvClient.GetMediaServers(includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(2);
            result[0].Name.Should().Be("Family Room");
            result[1].Name.Should().Be("Master Bedroom");
        }

        [TestMethod]
        public void GetMediaServers_ShouldExcludeSpecifiedShows()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetMediaServers()).Returns(new MediaServer[] {
                new MediaServer() { Name = "Family Room" },
                new MediaServer() { Name = "Master Bedroom" },
                new MediaServer() { Name = "Basement" }
            }.ToList());
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(It.IsAny<MediaServer>())).Returns(true);
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "*"; // include everything
            var excludeFilter = "*oom";

            // Act
            var result = tvClient.GetMediaServers(includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(1);
            result[0].Name.Should().Be("Basement");
        }

        [TestMethod]
        public void GetMediaServers_ShouldHaveExcludeOverrideInclude()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetMediaServers()).Returns(new MediaServer[] {
                new MediaServer() { Name = "Family Room" },
                new MediaServer() { Name = "Master Bedroom" },
                new MediaServer() { Name = "Basement" }
            }.ToList());
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(It.IsAny<MediaServer>())).Returns(true);
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);
            var includeFilter = "*oom";
            var excludeFilter = "Master*";

            // Act
            var result = tvClient.GetMediaServers(includeFilter, excludeFilter).ToList();

            // Assert
            result.Count.Should().Be(1);
            result[0].Name.Should().Be("Family Room");
        }

        [TestMethod]
        public void GetMediaServers_ShouldLocateAllMediaServers()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            tvHttpClientMock.Setup(c => c.GetMediaServers()).Returns(new MediaServer[] {
                new MediaServer() { Name = "Family Room" },
                new MediaServer() { Name = "Master Bedroom" },
                new MediaServer() { Name = "Basement" }
            }.ToList());
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(It.IsAny<MediaServer>())).Returns(true);
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);

            // Act
            tvClient.GetMediaServers().ToList();

            // Assert
            tvHttpClientMock.Verify(c => c.LocateMediaServer(It.IsAny<MediaServer>()), Times.Exactly(3));
        }

        [TestMethod]
        public void GetMediaServers_ShouldOnlyIncludeMediaServersThatCanBePinged()
        {
            // Arrange
            var tvHttpClientMock = new Mock<ISimpleTvHttpClient>();
            var mediaServers = new MediaServer[] {
                new MediaServer() { Name = "Family Room" },
                new MediaServer() { Name = "Master Bedroom" },
                new MediaServer() { Name = "Basement" }
            }.ToList();
            tvHttpClientMock.Setup(c => c.GetMediaServers()).Returns(mediaServers);
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(mediaServers[0])).Returns(true);
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(mediaServers[1])).Returns(false);
            tvHttpClientMock.Setup(c => c.TestMediaServerLocations(mediaServers[2])).Returns(true);
            var tvClient = new SimpleTvClient(tvHttpClientMock.Object);

            // Act
            var result = tvClient.GetMediaServers().ToList();

            // Assert
            result.Count().Should().Be(2);
            result[0].Name.Should().Be("Family Room");
            result[1].Name.Should().Be("Basement");
        }


    }
}
