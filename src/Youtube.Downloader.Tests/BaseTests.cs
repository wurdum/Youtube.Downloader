using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Youtube.Downloader.Tests
{
    class BaseTests
    {
        private static Dictionary<string, Video> Cache = new Dictionary<string, Video>();

        private const string TempDirName = "tmp";
        private const string ResourceDirName = "Resources";
        private const string TemplateVideo = "Missing.mp4";
        private string _resourcesDir;
        private string _tempDir;

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            var assemblyLocation = Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
            _tempDir = Path.Combine(assemblyLocation, TempDirName);
            _resourcesDir = Path.Combine(assemblyLocation, ResourceDirName);

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);

            Directory.CreateDirectory(_tempDir);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void VideoFormatsTest() {
            var url = "http://www.youtube.com/watch?v=KPWi3EKqNmU";
            var video = LoadOrGetFromCache(url);

            Assert.IsTrue(video.Formats.Count == 12);
            Assert.IsTrue(video.Title.Equals("Пятничный подкаст [Много нового]"));
        }
        
        [Test]
        [TestCase(VideoQuality.Low, 360)]
        [TestCase(VideoQuality.Medium, 720)]
        [TestCase(VideoQuality.High, 1080)]
        public void VideoQualityTest(VideoQuality quality, int resolution) {
            var url = "http://www.youtube.com/watch?v=KPWi3EKqNmU";
            var video = LoadOrGetFromCache(url);

            var format = video.GetMp4(quality);

            Assert.AreEqual(format.Resolution, resolution);
        }

        [Test]
        public void DownloadVideoTest() {
            var url = "http://www.youtube.com/watch?v=UiyDmqO59QE";
            var video = LoadOrGetFromCache(url);

            var format = video.Formats.First(v => v.VideoType == VideoType.Mp4 && v.Resolution == 1080);
            var savePath = Path.Combine(_tempDir, video.Title + format.VideoExtension);

            var videoDownloader = new VideoDownloader(format, savePath);
            Task.WaitAll(videoDownloader.Execute());

            var downloadedBytes = GetVideoBytes(Path.Combine(_resourcesDir, TemplateVideo));
            var templateBytes = GetVideoBytes(savePath);
            Assert.AreEqual(downloadedBytes.Length, templateBytes.Length);
        }

        private byte[] GetVideoBytes(string path) {
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                var buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int) reader.Length);
                
                return buffer;
            }
        }

        [Test]
        public void TaskExecutionTest() {
            var url = "http://www.youtube.com/watch?v=UiyDmqO59QE";
            var video = LoadOrGetFromCache(url);

            var format = video.Formats.First(v => v.VideoType == VideoType.Mp4 && v.Resolution == 1080);
            var savePath = Path.Combine(_tempDir, video.Title + format.VideoExtension);

            var videoDownloader = new VideoDownloader(format, savePath);
            videoDownloader.ProgressChanged += (o, a) => Console.WriteLine(a);
            videoDownloader.Execute().Wait();
        }

        public static Video LoadOrGetFromCache(string url) {
            Video video;
            if (Cache.TryGetValue(url, out video))
                return video;

            video = Video.Factory.Create(url).LoadVideo();
            Cache.Add(url, video);
            return video;
        }
    }
}
