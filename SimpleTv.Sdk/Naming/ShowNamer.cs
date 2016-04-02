using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using SimpleTv.Sdk.Models;

namespace SimpleTv.Sdk.Naming
{
    public static class ShowNamer
    {
        public static string GenerateFileName(this Episode episode, string baseFolder, string folderFormat, string fileNameTemplate) {
            var tokens = episode.GenerateTokens();

            return Path.Combine(
                baseFolder.ReplaceWithTokens(tokens, PathCleaner.CleanFolderName),
                folderFormat.ReplaceWithTokens(tokens, PathCleaner.CleanFolderName),
                fileNameTemplate.ReplaceWithTokens(tokens, PathCleaner.CleanFileName)
            );
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
