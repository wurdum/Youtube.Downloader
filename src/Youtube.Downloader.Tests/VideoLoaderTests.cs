using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Helpers;
using Youtube.Downloader.Loaders;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class VideoLoaderTests
    {
        [TestCase("")]
        [TestCase("notyoutube.com")]
        [TestCase("youtube.com")]
        [TestCase("youtube.com/watch?x=sadasd")]
        public void LoaderReadsOnlyYoutubeLisks(string url) {
            Assert.Throws<ArgumentException>(() => {
                var loader = new VideoLoader();
                loader.LoadVideo(url);
            });
        }

        [Test]
        public void UnavailableVideosThrows() {
            var pageLoader = new Mock<HttpLoader>();
            pageLoader.Setup(p => p.GetPage(It.IsAny<string>()))
                .Returns(@"<html>
	                            <head></head>
	                            <body>
		                            <div id=""watch-player-unavailable"">
	                            </body>
                            </html>");

            Assert.Throws<VideoNotAvailableException>(() => {
                var loader = new VideoLoader(new HttpUtilities(), pageLoader.Object);
                loader.LoadVideo("youtube.com/watch?v=x");
            });
        }

        [TestCaseSource("TestPages")]
        public Video VideoLoadingTest(string url, string page) {
            var pagesLoader = new Mock<HttpLoader>();
            pagesLoader.Setup(p => p.GetPage(It.IsAny<string>())).Returns(page);
            var loader = new VideoLoader(new HttpUtilities(), pagesLoader.Object);

            return loader.LoadVideo(url);
        }

        public IEnumerable<TestCaseData> TestPages {
            get {
                yield return new TestCaseData("youtube.com/watch?v=xxx", @"<html>
	                                                                        <head>
		                                                                        <meta name=""title"" content=""xxx"">
	                                                                        </head>
	                                                                        <body></body>
                                                                        </html>").Returns(new Video("xxx", "xxx"));
                yield return new TestCaseData("youtube.com/watch?v=zx-zx", @"<html>
	                                                                        <head>
		                                                                        <meta name=""title"" content=""dmdmdmd"">
	                                                                        </head>
	                                                                        <body></body>
                                                                        </html>").Returns(new Video("zx-zx", "dmdmdmd"));
                yield return new TestCaseData("youtube.com/watch?v=zx2-zx2", @"<html>
	                                                                        <head>
	                                                                        </head>
	                                                                        <body></body>
                                                                        </html>").Throws(typeof(YoutubeParseException));
            }
        }
    }
}