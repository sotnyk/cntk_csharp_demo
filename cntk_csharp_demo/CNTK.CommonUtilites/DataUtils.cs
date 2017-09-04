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

        public static int SwapEndianness(this int value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }
    }
}
