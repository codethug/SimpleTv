using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTvSdk.Tests.Helpers
{
    public class SampleData
    {
        public static string Get(string filename)
        {
            // var names = (new SampleData()).GetType().Assembly.GetManifestResourceNames();

            string result = string.Empty;

            var name = "SimpleTv.Sdk.Tests." + filename;
            using (Stream stream = (new SampleData()).GetType().Assembly.GetManifestResourceStream(name))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
