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

    }
}
