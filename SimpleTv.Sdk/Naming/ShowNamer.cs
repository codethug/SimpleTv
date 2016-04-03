using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using SimpleTv.Sdk.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleTv.Sdk.Naming
{
    public static class ShowNamer
    {
        // Todo: add information on new token to documentation (only works with filename, not folders)
        // Todo: Remove "{nnnn}" magic string
        // Todo: Make "{nnnn}" more flexible in regargs to number of 'n's
        public static string GenerateFileName(this Episode episode, string baseFolder, string folderFormat, string fileNameTemplate)
        {
            var tokens = episode.GenerateTokens();

            var folder = Path.Combine(
                baseFolder.ReplaceWithTokens(tokens, PathCleaner.CleanFolderName),
                folderFormat.ReplaceWithTokens(tokens, PathCleaner.CleanFolderName)
            );

            var fileName = fileNameTemplate.ReplaceWithTokens(tokens, PathCleaner.CleanFileName);
            var sequenceToken = "{nnnn}";
            if (fileName.Contains(sequenceToken))
            {
                var maxNum = GetMaxNumberedFileName(folder, fileName, sequenceToken);
                var newNumFormatted = (maxNum + 1).ToString("D4");  // 85 => "0085"
                fileName = fileName.Replace(sequenceToken, newNumFormatted);
            }

            return Path.Combine(folder, fileName);
        }

        public static int GetMaxNumberedFileName(string folder, string fileNameFormat, string token)
        {
            return Directory
                .GetFiles(folder, fileNameFormat.Replace(token, "*"), SearchOption.TopDirectoryOnly)
                .Max(f => GetSequenceNumberFromFileName(fileNameFormat, token, Path.GetFileName(f)));
        }

        /// <summary>Find the file with the largest sequence number matching the pattern</summary>
        /// <returns>The sequence number of the file, or 0 if no files found that match the pattern.</returns>
        /// <example>
        /// fileNamePattern: GreatShow-{nnnn}.mp4
        /// fileName: GreatShow-0013.mp4
        /// returns: 13
        /// </example>
        public static int GetSequenceNumberFromFileName(string fileNamePattern, string token, string fileName)
        {
            // Create a regex to find the number from the filename
            // We want to escape the fileNamePattern string so that characters (such as '.') are 
            // escaped, but we don't want to escape the part where we are searching for the sequence
            // number - the place where the fileNamePattern has "{nnnn}".  So we'll replace the "{nnnn}"
            // temporarily so it doesn't get escaped then we'll put back in the proper regex
            // text of (\d+) to search for the sequence number.
            const string sequenceIdentifier = "__TVDOWNLOADERSEQUENCENUMBER__";
            var pattern = fileNamePattern.Replace(token, sequenceIdentifier);
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace(sequenceIdentifier, @"(\d+)");
            var regex = new Regex(pattern);

            int result = 0;

            var match = regex.Match(fileName);
            if (match.Success)
            {
                var regExResult = match.Groups[1].Value;
                Int32.TryParse(regExResult, out result);
            }

            return result;
        }


        private static string ReplaceWithTokens(this string input, Dictionary<string, string> tokens, Func<string, string> tokenCleaner)
        {
            foreach (var token in tokens)
            {
                var cleanTokenValue = tokenCleaner(token.Value ?? string.Empty);
                input = input.Replace("{" + token.Key + "}", cleanTokenValue);
            }
            return input;
        }

        private static Dictionary<string, string> GenerateTokens(this Episode episode)
        {
            var tokens = new Dictionary<string, string>();

            tokens.Add("ShowName",          HttpUtility.HtmlDecode(episode.show.Name));
            tokens.Add("SeasonNumber",      episode.SeasonNumber.ToString());       // 3 => "3"
            tokens.Add("EpisodeNumber",     episode.EpisodeNumber.ToString());      // 5 => "5"
            tokens.Add("SeasonNumber00",    episode.SeasonNumber.ToString("D2"));   // 3 => "03"
            tokens.Add("EpisodeNumber00",   episode.EpisodeNumber.ToString("D2"));  // 3 => "03"
            tokens.Add("EpisodeName",       HttpUtility.HtmlDecode(episode.EpisodeName));

            return tokens;
        }
    }
}
