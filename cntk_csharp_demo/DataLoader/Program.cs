using CNTK.CommonUtilites;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataLoader
{
    class Program
    {
        static async Task TryDownloadAsync(string dataSrc, string labelsSrc, int imagesNum)
        {
            var dataTask = TryLoadDataAsync(dataSrc);
            var labelTask = TryLoadDataAsync(labelsSrc);
            await Task.WhenAll(dataTask, labelTask);
        }

        private static async Task TryLoadDataAsync(string dataSrc)
        {
            var filename = Path.Combine(DataUtils.DataPath, new Uri(dataSrc).Segments.Last());
            if (File.Exists(filename))
            {
                Console.WriteLine($"File {filename} has been already downloaded.");
            }
            else
            {
                Console.WriteLine($"Starting downloading of {dataSrc}.");
                var client = new HttpClient();
                var data = await client.GetByteArrayAsync(dataSrc);
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
            var trainTask = TryDownloadAsync(urlTrainImage, urlTrainLabels, numTrainSamples);

            Console.WriteLine("Download test data");
            var urlTestImage = "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz";
            var urlTestLabels = "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz";
            var numTestSamples = 10000;
            var testTask = TryDownloadAsync(urlTestImage, urlTestLabels, numTestSamples);

            Task.WaitAll(trainTask, testTask);

            Console.ReadLine();
        }
    }
}
