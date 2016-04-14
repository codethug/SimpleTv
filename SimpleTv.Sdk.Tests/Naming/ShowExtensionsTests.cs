using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Naming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Tests.Naming
{
    [TestClass]
    public class ShowExtensionsTests
    {
        [TestMethod]
        public void IncludeOnly_ShouldFilterOutUnmatchedShows()
        {
            // Arrange
            var allShows = new Show[] {
                new Show(null, null) { Name = "AA1" },
                new Show(null, null) { Name = "AA2" },
                new Show(null, null) { Name = "BB1" },
                new Show(null, null) { Name = "BB2" },
            };

            // Act 
            var filteredShows = allShows.IncludeOnly("AA*").ToList();

            // Assert
            filteredShows.Count.Should().Be(2);
            filteredShows[0].Name.Should().Be("AA1");
            filteredShows[1].Name.Should().Be("AA2");
        }

        [TestMethod]
        public void Exclude_ShouldFilterOutMatchedShows()
        {
            // Arrange
            var allShows = new Show[] {
                new Show(null, null) { Name = "AA1" },
                new Show(null, null) { Name = "AA2" },
                new Show(null, null) { Name = "BB1" },
                new Show(null, null) { Name = "BB2" },
            };

            // Act 
            var filteredShows = allShows.Exclude("AA*").ToList();

            // Assert
            filteredShows.Count.Should().Be(2);
            filteredShows[0].Name.Should().Be("BB1");
            filteredShows[1].Name.Should().Be("BB2");
        }

    }
}
