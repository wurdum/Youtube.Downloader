using Moq;
using NUnit.Framework;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Helpers;
using Youtube.Downloader.Loaders;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class FormatsLoaderTests
    {
        private const string formatsPage = "status=ok&url_encoded_fmt_stream_map=" +
                                           "sig%3Dxxx%26url%3Dhttp%253A%252F%252Fxxx.com%252Fx%253Fitag%253D46%26fallback_host%3Dxxx%2C" +
                                           "sig%3Dyyy%26url%3Dhttp%253A%252F%252Fyyy.com%252Fx%253Fitag%253D82%26fallback_host%3Dyyy%2C" +
                                           "sig%3Dzzz%26url%3Dhttp%253A%252F%252Fzzz.com%252Fx%253Fitag%253D84%26fallback_host%3Dzzz";
        private readonly FormatsList _formatsList = new FormatsList {
            new Format(46, VideoType.WebM, 1080) {DownloadUrl = "http://xxx.com/x?itag=46&fallback_host=xxx&signature=xxx"},
            new Format(82, VideoType.Mp4, 360) {DownloadUrl = "http://yyy.com/x?itag=82&fallback_host=yyy&signature=yyy"},
            new Format(84, VideoType.Mp4, 720) {DownloadUrl = "http://zzz.com/x?itag=84&fallback_host=zzz&signature=zzz"},
        };

        [Test]
        public void IfNoFormatsInResponsePageThrow() {
            var pagesLoader = new Mock<HttpLoader>();
            pagesLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns("status=fail");
            var video = new Video("xxx", "xxx");
            var formatsLoader = new FormatsLoader(new HttpUtilities(), pagesLoader.Object);

            Assert.Throws<FormatsLoadingException>(() => formatsLoader.LoadFormats(video));
        }

        [Test]
        public void WrongFormatsPageFormatLoadingThrows() {
            var pagesLoader = new Mock<HttpLoader>();
            pagesLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns("status=ok&afasfasfasfasfsfsf");
            var video = new Video("xxx", "xxx");
            var formatsLoader = new FormatsLoader(new HttpUtilities(), pagesLoader.Object);

            Assert.Throws<FormatsLoadingException>(() => formatsLoader.LoadFormats(video));
        }

        [Test]
        public void FormatsQualityRetrievingTest() {
            var pagesLoader = new Mock<HttpLoader>();
            pagesLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(formatsPage);
            var video = new Video("xxx", "xxx");
            var formatsLoader = new FormatsLoader(new HttpUtilities(), pagesLoader.Object);

            var resultFormats = formatsLoader.LoadFormats(video);

            Assert.AreEqual(360, resultFormats.GetMp4(VideoQuality.Low).Resolution);
            Assert.AreEqual(720, resultFormats.GetMp4(VideoQuality.High).Resolution);
        }

        [Test]
        public void FormatsPageParsing() {
            var pagesLoader = new Mock<HttpLoader>();
            pagesLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(formatsPage);
            var video = new Video("xxx", "xxx");
            var formatsLoader = new FormatsLoader(new HttpUtilities(), pagesLoader.Object);

            var resultFormats = formatsLoader.LoadFormats(video);
            Assert.AreEqual(_formatsList, resultFormats);
        }
    }
}