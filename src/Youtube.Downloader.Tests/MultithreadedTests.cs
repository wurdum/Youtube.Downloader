using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [Test]
        public void MainTest() {
            RunInThreads(data => () => {
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

                var videoLoader = new VideoLoader(new HttpUtilities(), videoPageLoader.Object);
                var video = videoLoader.LoadVideo(data.VideoUrl);
                var formatsLoader = new FormatsLoader(new HttpUtilities(), formatsPageLoader.Object);
                video.LoadFormats(formatsLoader);
                var format = video.FormatsList.GetMp4(VideoQuality.High);

                Assert.AreEqual(data.DownloadUrl, format.DownloadUrl, data.VideoUrl);
            }, 2000);
        }

        [Test]
        public void FormatsLoaderTest() {
            RunInThreads(data => () => {
                var randomizer = Rnd.Value;

                var formatsPageLoader = new Mock<HttpLoader>();
                formatsPageLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(() => {
                    Thread.Sleep(randomizer.Next(5));
                    return data.FormatsUrl;
                });

                var formatsLoader = new FormatsLoader(new HttpUtilities(), formatsPageLoader.Object);
                var formats = formatsLoader.LoadFormats(new Video(data.DownloadUrl.Substring(27), ""));
                var format = formats.GetMp4(VideoQuality.High);

                Assert.AreEqual(data.DownloadUrl, format.DownloadUrl, data.VideoUrl);
            }, 2000);
        }

        [Test]
        public void HttpUtilityParseQueryStringTest() {
            RunInThreads(data => () => {
                var parameters = new HttpUtilities().ParseQueryString(new Uri(data.VideoUrl).Query, Encoding.UTF8);

                Assert.AreEqual(parameters["v"], data.VideoUrl.Substring(27));
            }, 10000);

        }

        [Test]
        public void HttpUtilityDecodeUrlsTest() {
            RunInThreads(data => () => {
                var decoded = new HttpUtilities().UrlDecode(data.FormatsUrl, Encoding.UTF8);

                Assert.AreEqual(decoded, data.FormatsUrlDecoded);
            }, 10000);            
        }

        private static void RunInThreads(Func<VideoData, Action> getBody, int runs) {
            for (var i = 0; i < runs; i++) {
                var tasks = VideosData.Select(data => new Task(getBody(data))).ToArray();

                foreach (var task in tasks)
                    task.Start();

                Task.WaitAll(tasks);
            }
        }

        private static readonly IList<VideoData> VideosData = new List<VideoData> {
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=xxxx", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Dxxx%26url%3Dhttp%253A%252F%252Fxxx.com%252Fx%253Fitag%253D46%26fallback_host%3Dxxx%2C" +
                             "sig%3Dyyy%26url%3Dhttp%253A%252F%252Fyyy.com%252Fx%253Fitag%253D82%26fallback_host%3Dyyy%2C" +
                             "sig%3Dzzz%26url%3Dhttp%253A%252F%252Fzzz.com%252Fx%253Fitag%253D84%26fallback_host%3Dzzz",
                FormatsUrlDecoded = "status=ok&url_encoded_fmt_stream_map=sig=xxx&url=http%3A%2F%2Fxxx.com%2Fx%3Fitag%3D46&fallback_host=xxx," +
                                    "sig=yyy&url=http%3A%2F%2Fyyy.com%2Fx%3Fitag%3D82&fallback_host=yyy,sig=zzz&url=http%3A%2F%2Fzzz.com%2Fx%3Fitag%3D84&fallback_host=zzz",
                DownloadUrl = "http://zzz.com/x?itag=84&fallback_host=zzz&signature=zzz"
            },
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=cccc", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Daaa%26url%3Dhttp%253A%252F%252Faaa.com%252Fx%253Fitag%253D46%26fallback_host%3Daaa%2C" +
                             "sig%3Dbbb%26url%3Dhttp%253A%252F%252Fbbb.com%252Fx%253Fitag%253D82%26fallback_host%3Dbbb%2C" +
                             "sig%3Dccc%26url%3Dhttp%253A%252F%252Fccc.com%252Fx%253Fitag%253D84%26fallback_host%3Dccc",
                FormatsUrlDecoded = "status=ok&url_encoded_fmt_stream_map=sig=aaa&url=http%3A%2F%2Faaa.com%2Fx%3Fitag%3D46&fallback_" +
                                    "host=aaa,sig=bbb&url=http%3A%2F%2Fbbb.com%2Fx%3Fitag%3D82&fallback_host=bbb,sig=ccc&url=http%3A%2" +
                                    "F%2Fccc.com%2Fx%3Fitag%3D84&fallback_host=ccc",
                DownloadUrl = "http://ccc.com/x?itag=84&fallback_host=ccc&signature=ccc"
            },
            new VideoData {
                VideoUrl = "http://youtube.com/watch?v=ffff", 
                FormatsUrl = "status=ok&url_encoded_fmt_stream_map=" +
                             "sig%3Dddd%26url%3Dhttp%253A%252F%252Fddd.com%252Fx%253Fitag%253D46%26fallback_host%3Dddd%2C" +
                             "sig%3Deee%26url%3Dhttp%253A%252F%252Feee.com%252Fx%253Fitag%253D82%26fallback_host%3Deee%2C" +
                             "sig%3Dfff%26url%3Dhttp%253A%252F%252Ffff.com%252Fx%253Fitag%253D84%26fallback_host%3Dfff",
                FormatsUrlDecoded = "status=ok&url_encoded_fmt_stream_map=sig=ddd&url=http%3A%2F%2Fddd.com%2Fx%3Fitag%3D46&fallback_ho" +
                                    "st=ddd,sig=eee&url=http%3A%2F%2Feee.com%2Fx%3Fitag%3D82&fallback_host=eee,sig=fff&url=http%3A%2F%2Ffff.com%2Fx%3Fitag%3D84&fallback_host=fff",
                DownloadUrl = "http://fff.com/x?itag=84&fallback_host=fff&signature=fff"
            }
        };

        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> Rnd = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        private class VideoData
        {
            public string VideoUrl { get; set; }
            public string FormatsUrl { get; set; }
            public string FormatsUrlDecoded { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}