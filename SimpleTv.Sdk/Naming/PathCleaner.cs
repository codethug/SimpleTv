using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Naming
{
    public static class PathCleaner
    {
        public static string CleanFolderName(this string path)
        {
            char[] customInvalidChars = { ':','<','>','|','\b','\t','*','?','\"','\0',
                Path.VolumeSeparatorChar,Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar };
            return path
                .RemoveCharacters(Path.GetInvalidPathChars())
                .RemoveCharacters(customInvalidChars);
        }

        public static string CleanFileName(this string path)
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
