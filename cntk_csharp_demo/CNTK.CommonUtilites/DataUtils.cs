using System;
using System.IO;

namespace CNTK.CommonUtilites
{
    public static class DataUtils
    {
        public static string DataPath { get; private set; }

        static DataUtils()
        {
            DataPath = FindDataDirectory();
        }

        private static string FindDataDirectory()
        {
            var curDir = AppContext.BaseDirectory;
            while (!string.IsNullOrWhiteSpace(curDir))
            {
                curDir = Directory.GetParent(curDir).FullName;
                if (string.IsNullOrWhiteSpace(curDir))
                    break;
                var testPath = Path.Combine(curDir, "data");
                if (Directory.Exists(testPath))
                    return testPath;
            }
            throw new DirectoryNotFoundException("Subdirectory 'Data' not found in one of parent directories.");
        }
    }
}
