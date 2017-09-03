using CNTK.CommonUtilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DataLoader
{
    class Program
    {
        static string FilenameFromUrl(string url)
        {
            return Path.Combine(DataUtils.DataPath, new Uri(url).Segments.Last());
        }

        static async Task TryDownloadAsync(string dataSrc, string labelsSrc, int imagesNum, string ctfFilename)
        {
            var outFilename = Path.Combine(DataUtils.DataPath, ctfFilename);
            if (File.Exists(outFilename))
            {
                Console.WriteLine($"File '{ctfFilename}' is already existed, skip downloading.");
                return;
            }
            var dataTask = TryLoadDataAsync(dataSrc);
            var labelTask = TryLoadDataAsync(labelsSrc);
            await Task.WhenAll(dataTask, labelTask).ConfigureAwait(false);

            Console.WriteLine($"Export data into CNTK Text Format - '{ctfFilename}'.");
            GzToCtf(dataSrc, labelsSrc, imagesNum, ctfFilename);
        }

        private static void GzToCtf(string dataSrc, string labelsSrc, int imagesNum, string ctfFilename)
        {
            byte[,] images = LoadImages(dataSrc, imagesNum);
            byte[] labels = LoadLabels(labelsSrc, imagesNum);
        }

        private static byte[] LoadLabels(string labelsSrc, int imagesNum)
        {
            var filename = FilenameFromUrl(labelsSrc);

            using (FileStream fs = File.OpenRead(filename))
            using (GZipStream uncompressed = new GZipStream(fs, CompressionMode.Decompress))
            {
                var br = new BinaryReader(uncompressed);
                // Read magic number
                var mark = br.ReadInt32();
                if (mark != 0x1080000)
                    throw new FormatException("File should be stated from magix constant 0x1080000");
                var n = br.ReadInt32();
                if (n != imagesNum)
                    throw new ArgumentOutOfRangeException(nameof(imagesNum), $"Expected {imagesNum} rows but {n} found.");

                return br.ReadBytes(imagesNum);
            }
        }

        private static byte[,] LoadImages(string dataSrc, int imagesNum)
        {
            var filename = FilenameFromUrl(dataSrc);
            var res = new byte[imagesNum, 27 * 27];

            using (FileStream fs = File.OpenRead(filename))
            using (GZipStream uncompressed = new GZipStream(fs, CompressionMode.Decompress))
            {
                var br = new BinaryReader(uncompressed);
                // Read magic number
                var mark = br.ReadInt32();
                if (mark != 0x3080000)
                    throw new FormatException("File should be stated from magix constant 0x3080000");
                var n = br.ReadInt32();
                if (n != imagesNum)
                    throw new ArgumentOutOfRangeException(nameof(imagesNum), $"Expected {imagesNum} images but {n} found.");
                var rows = br.ReadInt32();
                if (rows != 28)
                    throw new FormatException($"Invalid file: expected 28 rows per image, but {rows} is found.");
                var cols = br.ReadInt32();
                if (cols != 28)
                    throw new FormatException($"Invalid file: expected 28 cols per image, but {cols} is found.");
                var data = br.ReadBytes(imagesNum * rows * cols);
                Array.Copy(data, res, data.Length);
            }

            return res;
        }

        private static async Task TryLoadDataAsync(string dataSrc)
        {
            var filename = Path.Combine(DataUtils.DataPath, FilenameFromUrl(dataSrc));
            if (File.Exists(filename))
            {
                Console.WriteLine($"File {filename} has been already downloaded.");
            }
            else
            {
                Console.WriteLine($"Starting downloading of {dataSrc}.");
                var client = new HttpClient();
                var data = await client.GetByteArrayAsync(dataSrc).ConfigureAwait(false);
                File.WriteAllBytes(filename, data);
                Console.WriteLine($"{dataSrc} has been downloaded ({data.Length} bytes) and stored to {filename}.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Download train data");
            var urlTrainImage = "http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz";
            var urlTrainLabels = "http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz";
            var numTrainSamples = 60000;
            var trainTask = TryDownloadAsync(urlTrainImage, urlTrainLabels, numTrainSamples, "Train-28x28_cntk_text.txt");

            Console.WriteLine("Download test data");
            var urlTestImage = "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz";
            var urlTestLabels = "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz";
            var numTestSamples = 10000;
            var testTask = TryDownloadAsync(urlTestImage, urlTestLabels, numTestSamples, "Test-28x28_cntk_text.txt");

            Task.WaitAll(trainTask, testTask);

            Console.ReadLine();
        }
    }
}
