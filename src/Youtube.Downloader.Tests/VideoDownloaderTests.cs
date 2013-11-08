using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Youtube.Downloader.Http;

namespace Youtube.Downloader.Tests
{
    class VideoDownloaderTests
    {
        private const string TempDirName = "tmp";
        private const string ResourceDirName = "Resources";
        private const string TemplateVideo = "Missing.mp4";
        private const string VideoUrl = "http://www.youtube.com/watch?v=UiyDmqO59QE";
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
            var id = UrlsComposer.ParseId(VideoUrl);
            
            var videoParser = new VideoParser(new HttpLoader(true), new HttpUtilities(), id);
            var video = videoParser.GetInBestQuality();
            
            var videoDownloader = new VideoDownloader(video, _tempDir);
            Task.WaitAll(videoDownloader.Execute());

            var downloadedBytes = GetVideoBytes(Path.Combine(_resourcesDir, TemplateVideo));
            var templateBytes = GetVideoBytes(videoDownloader.SavePath);
            Assert.AreEqual(downloadedBytes.Length, templateBytes.Length);
        }

        private byte[] GetVideoBytes(string path) {
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                var buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int) reader.Length);
                
                return buffer;
            }
        }
    }
}
