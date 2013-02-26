using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Youtube.Downloader.Helpers;
using Youtube.Downloader.Loaders;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class MultithreadedTests
    {
        private object _syncRoot = new object();

        [Test]
        public void MainTest() {
            for (var i = 0; i < 1000; i++) {
                var tasks = new List<Task>();

                foreach (var d in VideosData) {
                    var data = d;
                    tasks.Add(new Task(() => {
                        var randomizer = Rnd.Value;

                        var videoPageLoader = new Mock<HttpLoader>();
                        videoPageLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(() => {
                            Thread.Sleep(randomizer.Next(5));
                            return @"<html><head><meta name=""title"" content=""" + data.VideoUrl + @"""></head><body></body></html>";
                        });

                        var formatsPageLoader = new Mock<HttpLoader>();
                        formatsPageLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(() => {
                            Thread.Sleep(randomizer.Next(5));
                            return data.FormatsUrl;
                        });

                        Video video;
                        Format format;
                        
                        lock (_syncRoot) {
                            var videoLoader = new VideoLoader(new HttpUtilities(), videoPageLoader.Object);
                            video = videoLoader.LoadVideo(data.VideoUrl);
                            var formatsLoader = new FormatsLoader(new HttpUtilities(), formatsPageLoader.Object);    

                            video.LoadFormats(formatsLoader);

                            format = video.FormatsList.GetMp4(VideoQuality.High);
                        }

                        Assert.AreEqual(data.DownloadUrl, format.DownloadUrl, data.VideoUrl);
                        Console.WriteLine("Thread " + data.VideoUrl + " complete");
                    }));
                }

                foreach (var task in tasks)
                    task.Start();

                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("Iteration " + i + " complete");
            }
        }

        private static readonly IList<VideoData> VideosData = new List<VideoData> {
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=xxxx", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Dxxx%26url%3Dhttp%253A%252F%252Fxxx.com%252Fx%253Fitag%253D46%26fallback_host%3Dxxx%2C" +
                             "sig%3Dyyy%26url%3Dhttp%253A%252F%252Fyyy.com%252Fx%253Fitag%253D82%26fallback_host%3Dyyy%2C" +
                             "sig%3Dzzz%26url%3Dhttp%253A%252F%252Fzzz.com%252Fx%253Fitag%253D84%26fallback_host%3Dzzz", 
                DownloadUrl = "http://zzz.com/x?itag=84&fallback_host=zzz&signature=zzz"
            },
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=cccc", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Daaa%26url%3Dhttp%253A%252F%252Faaa.com%252Fx%253Fitag%253D46%26fallback_host%3Daaa%2C" +
                             "sig%3Dbbb%26url%3Dhttp%253A%252F%252Fbbb.com%252Fx%253Fitag%253D82%26fallback_host%3Dbbb%2C" +
                             "sig%3Dccc%26url%3Dhttp%253A%252F%252Fccc.com%252Fx%253Fitag%253D84%26fallback_host%3Dccc", 
                DownloadUrl = "http://ccc.com/x?itag=84&fallback_host=ccc&signature=ccc"
            },
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=ffff", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Dddd%26url%3Dhttp%253A%252F%252Fddd.com%252Fx%253Fitag%253D46%26fallback_host%3Dddd%2C" +
                             "sig%3Deee%26url%3Dhttp%253A%252F%252Feee.com%252Fx%253Fitag%253D82%26fallback_host%3Deee%2C" +
                             "sig%3Dfff%26url%3Dhttp%253A%252F%252Ffff.com%252Fx%253Fitag%253D84%26fallback_host%3Dfff", 
                DownloadUrl = "http://fff.com/x?itag=84&fallback_host=fff&signature=fff"
            }
        };

        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> Rnd = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        private class VideoData
        {
            public string VideoUrl { get; set; }
            public string FormatsUrl { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}