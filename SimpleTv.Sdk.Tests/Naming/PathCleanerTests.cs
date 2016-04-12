using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using SimpleTv.Sdk.Naming;

namespace SimpleTvSdk.Tests
{
    [TestClass]
    public class PathCleanerTests
    {
        [TestMethod]
        public void CleanFolderName_ShouldCleanPipeAndColon()
        {
            // Arrange
            var folder = "Hello:Folder|Name";

            // Act
            var cleanFolder = folder.CleanFolderName();

            // Assert
            cleanFolder.Should().Be("Hello_Folder_Name");
        }

        [TestMethod]
        public void CleanFileName_ShouldCleanPipeAndColon()
        {
            // Arrange
            var folder = "Hello:Folder|Name";

            // Act
            var cleanFolder = folder.CleanFileName();

            // Assert
            cleanFolder.Should().Be("Hello_Folder_Name");
        }

        [TestMethod]
        public void RegExReplace_ShouldPerformReplacement()
        {
            // Arrange
            var input = "/22000b6981a6/tv.main.hls-0.m3u8";
            var pattern = @"tv\.main\.hls-(\d)\.m3u8";
            var replacement = @"tv.4500000.10$1";

            // Act
            var result = input.RegexReplace(pattern, replacement);

            // Assert
            result.Should().Be("/22000b6981a6/tv.4500000.100");
        }
    }
}
