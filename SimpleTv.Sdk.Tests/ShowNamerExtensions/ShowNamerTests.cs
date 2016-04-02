using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Naming;

namespace SimpleTvSdk.Tests.ShowNamerExtensions
{
    [TestClass]
    public class ShowNamerTests
    {
        /// <summary>
        /// Issue 21 - https://github.com/codethug/SimpleTv/issues/21
        /// </summary>
        [TestMethod]
        public void GenerateFileName_ShouldNotStripOutDriveColonAndSlash()
        {
            // Arrange
            var episode = new Episode(null, new Show(null, null));
            var baseFolder = @"F:\";
            var folderFormat = "folder";
            var fileNameFormat = "file";

            // Act
            var name = episode.GenerateFileName(baseFolder, folderFormat, fileNameFormat);

            // Assert
            name.Should().Be(@"F:\folder\file");
        }
    }
}
