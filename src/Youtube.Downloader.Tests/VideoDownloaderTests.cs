using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Youtube.Downloader.Loaders;

namespace Youtube.Downloader.Tests
{
    class VideoDownloaderTests
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
        public void DownloadVideoTest() {
            var url = "http://www.youtube.com/watch?v=UiyDmqO59QE";
            var video = LoadOrGetFromCache(url);

            var format = video.FormatsList.First(v => v.VideoType == VideoType.Mp4 && v.Resolution == 1080);
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

        public static Video LoadOrGetFromCache(string url) {
            Video video;
            if (Cache.TryGetValue(url, out video))
                return video;

            var videoLoader = new VideoLoader();
            video = videoLoader.LoadVideo(url);
            video.LoadFormats(new FormatsLoader());
            Cache.Add(url, video);
            return video;
        }
    }
}
