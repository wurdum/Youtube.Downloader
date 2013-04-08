using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Youtube.Downloader.Loaders;
using Out = System.Console;

namespace Youtube.Downloader.Console
{
    class Program
    {
        private static readonly object SyncRoot = new object();

        static void Main(string[] urls) {
            WriteSeparator("Configuring");
            urls = ReadUrls(urls);
            var quality = ReadQuality();

            WriteSeparator("Downloading");
            var zeroCursorPosition = Out.CursorTop;
            var downloadingTasks = new List<Task>();
            for (var index = 0; index < urls.Length; index++) {
                var url = urls[index];
                var top = index;
                downloadingTasks.Add(new Task(() => {
                    var videoLoader = new VideoLoader();
                    var video = videoLoader.LoadVideo(url);
                    video.LoadFormats(new FormatsLoader());
                    var format = video.FormatsList.GetMp4(quality);

                    var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), video.Title + format.VideoExtension);
                    var videoDownloader = new VideoDownloader(format, savePath);
                    videoDownloader.ProgressChanged += (o, a) => {
                        lock (SyncRoot) {
                            Out.SetCursorPosition(0, zeroCursorPosition + top);
                            Out.ForegroundColor = ConsoleColor.Green;
                            Out.Write("{0}%\t", a.ProgressPercentage);
                            Out.ForegroundColor = ConsoleColor.White;
                            Out.Write(video.Title + '\n');
                        }
                    };

                    videoDownloader.Execute().Wait();
                }));
            }

            foreach (var task in downloadingTasks)
                task.Start();

            Task.WaitAll(downloadingTasks.ToArray());

            Out.WriteLine("\nDone!");
        }

        private static void WriteSeparator(string msg) {
            for (var i = 0; i < (Out.WindowWidth/2) - 10; i++)
                Out.Write('-');
            Out.Write(msg);
            for (var i = 0; i < (Out.WindowWidth/2) - 10; i++)
                Out.Write('-');
            Out.Write('\n');
        }

        private static VideoQuality ReadQuality() {
            while (true) {
                Out.WriteLine("Please choose video quality: ");
                Out.WriteLine("1. Low");
                Out.WriteLine("2. Medium");
                Out.WriteLine("3. High");
                Out.Write("Your choise: ");
                int answer;
                if (Int32.TryParse(Out.ReadLine(), out answer) && new[] {1, 2, 3}.Contains(answer))
                    return (VideoQuality) answer;
            }
        }

        private static string[] ReadUrls(string[] urls) {
            while (urls.Length == 0) {
                Out.WriteLine("Please specify youtube videos to download:");
                var videos = urls.ToList();
                var counter = 0;
                do {
                    Out.Write("Video[{0}]: ", counter++);
                    var url = Out.ReadLine();
                    if (string.IsNullOrEmpty(url))
                        break;
                    videos.Add(url);
                } while (true);

                urls = videos.ToArray();
            }

            return urls;
        }
    }
}
