using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using SimpleTv.Sdk.Models;
using SimpleTv.Sdk.Naming;
using System;
using System.IO;
using SimpleTvSdk.Tests.Helpers;

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

        [TestMethod]
        public void GetMaxNumberedFileName_ShouldFindLargestOf3FileNamesMatchingPattern()
        {
            // Arrange
            using (var tempFolder = new TestingFolder())
            {
                var fileNameFormat = "TestFileName-{nnnn}.mp4";
                var token = "{nnnn}";
                File.Create(Path.Combine(tempFolder.Location, "TestFileName-0012.mp4")).Dispose();
                File.Create(Path.Combine(tempFolder.Location, "TestFileName-0025.mp4")).Dispose();
                File.Create(Path.Combine(tempFolder.Location, "TestFileName-0045.mp4")).Dispose();

                // Act
                var maxNum = ShowNamer.GetMaxNumberedFileName(tempFolder.Location, fileNameFormat, token);

                // Assert
                maxNum.Should().Be(45);
            }
        }

        [TestMethod]
        public void GetSequenceNumberFromFileName_ShouldGetSequenceNumberFromFileName()
        {
            // Arrange
            var fileNamePattern = "TestName-{nnnn}.mp4";
            var token = "{nnnn}";
            var filename = "TestName-0123.mp4";

            // Act
            var sequenceNumber = ShowNamer.GetSequenceNumberFromFileName(fileNamePattern, token, filename);

            // Assert
            sequenceNumber.Should().Be(123);
        }

        [TestMethod]
        public void GetSequenceNumberFromFileName_ShouldWorkWithTooMuchPadding()
        {
            // Arrange
            var fileNamePattern = "TestName-{nnnn}.mp4";
            var token = "{nnnn}";
            var filename = "TestName-0000123.mp4";

            // Act
            var sequenceNumber = ShowNamer.GetSequenceNumberFromFileName(fileNamePattern, token, filename);

            // Assert
            sequenceNumber.Should().Be(123);
        }

        [TestMethod]
        public void GetSequenceNumberFromFileName_ShouldWorkWithTooLittlePadding()
        {
            // Arrange
            var fileNamePattern = "TestName-{nnnn}.mp4";
            var token = "{nnnn}";
            var filename = "TestName-3.mp4";

            // Act
            var sequenceNumber = ShowNamer.GetSequenceNumberFromFileName(fileNamePattern, token, filename);

            // Assert
            sequenceNumber.Should().Be(3);
        }

        [TestMethod]
        public void GenerateTokens_ShouldGenerateChannelNumberFromEpisode()
        {
            // Arrange
            var episode = new Episode(null, null)
            {
                ChannelNumber = "97523.12345"
            };

            // Act
            var tokens = episode.GenerateTokens();

            // Assert
            tokens["ChannelNumber"].Should().Be("97523.12345");
        }

        [TestMethod]
        public void GenerateTokens_ShouldGenerateDateTimeFromEpisode()
        {
            // Arrange
            var episode = new Episode(null, null)
            {
                DateTime = new DateTime(2000, 08, 15, 16, 53, 00)
            };

            // Act
            var tokens = episode.GenerateTokens();

            // Assert
            tokens["DateTime"].Should().Be("2000-08-15T1653");
        }
    }
}
