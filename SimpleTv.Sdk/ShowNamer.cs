using System.Collections.Generic;
using System.IO;
using System.Web;

using SimpleTv.Sdk.Models;

namespace SimpleTv.Sdk
{
    public static class ShowNamer
    {
        public static string GenerateFileName(this Episode episode, string baseFolder, string folderFormat, string fileNameTemplate) {
            var tokens = episode.GenerateTokens();

            return Path.Combine(
                baseFolder.ReplaceWithTokens(tokens).RemoveInvalidFolderCharacters(),
                folderFormat.ReplaceWithTokens(tokens).RemoveInvalidFolderCharacters(),
                fileNameTemplate.ReplaceWithTokens(tokens).RemoveInvalidFileCharacters()
            );
        }

        private static string ReplaceWithTokens(this string input, Dictionary<string,string> tokens)
        {
            foreach (var token in tokens)
            {
                input = input.Replace("{" + token.Key + "}", token.Value);
            }
            return input;
        }

        private static Dictionary<string, string> GenerateTokens(this Episode episode)
        {
            var tokens = new Dictionary<string, string>();
            tokens.Add("ShowName", HttpUtility.HtmlDecode(episode.show.Name));
            tokens.Add("SeasonNumber00", episode.SeasonNumber.ToString("D2")); // always have 2 numerals
            tokens.Add("EpisodeNumber00", episode.EpisodeNumber.ToString("D2"));
            tokens.Add("EpisodeName", HttpUtility.HtmlDecode(episode.EpisodeName));
            return tokens;
        }

        private static string RemoveInvalidFolderCharacters(this string path)
        {
            return path.RemoveCharacters(Path.GetInvalidPathChars());
        }

        private static string RemoveInvalidFileCharacters(this string path)
        {
            return path.RemoveCharacters(Path.GetInvalidFileNameChars());
        }

        private static string RemoveCharacters(this string input, char[] chars)
        {
            string output = input;
            foreach (var c in chars)
            {
                output = output.Replace(c, '_');
            }
            return output;
        }

    }
}
