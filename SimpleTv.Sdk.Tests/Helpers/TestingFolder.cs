using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTvSdk.Tests.Helpers
{
    public class TestingFolder : IDisposable
    {
        public string Location { get; private set; }

        public TestingFolder()
        {
            // Create Temporary Folder
            Location = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(Location);
        }

        public void Dispose()
        {
            // Delete Temporary Folder
            Directory.Delete(Location, true);
        }
    }
}
