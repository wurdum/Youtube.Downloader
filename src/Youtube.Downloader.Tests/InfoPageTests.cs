using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Youtube.Downloader.Http;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class InfoPageTests
    {
        [Test]
        public void TR() {
            InfoPage ip = new InfoPage(new HttpLoader(true), new HttpUtilities(), "UiyDmqO59QE");
            ip.GetVideoUrls();
        }

        [Test, TestCaseSource("GetInfoPages")]
        public void TitleParsingTest(InfoPage infoPage, Dictionary<string, string> info) {
            Assert.AreEqual(infoPage.GetTitle(), info["title"]);
        }

        [Test, TestCaseSource("GetInfoPages")]
        public void UrlsParsingTest(InfoPage infoPage, Dictionary<string, string> info) {
            CollectionAssert.AreEquivalent(infoPage.GetVideoUrls(), LoadUrlsFromResources(info["urls"]));
        }

        private static IEnumerable GetInfoPages {
            get {
                yield return new TestCaseData(
                    new InfoPage(GetHttpLoaderMock(InfoPageFromResources("Missing")), new HttpUtilities(), "xxx"),
                    new Dictionary<string, string> {
                        {"title", "Missing"},
                        {"urls", "Missing"}
                    }
                );
                yield return new TestCaseData(
                    new InfoPage(GetHttpLoaderMock(InfoPageFromResources("BlackGTA")), new HttpUtilities(), "xxx"),
                    new Dictionary<string, string> {
                        {"title", "Суровые будни в GTA Online #22 - С ножом на дробовик"},
                        {"urls", "BlackGTA"}
                    }
                );
                yield return new TestCaseData(
                    new InfoPage(GetHttpLoaderMock(InfoPageFromResources("RJ")), new HttpUtilities(), "xxx"),
                    new Dictionary<string, string> {
                        {"title", "Potato digging 【いもほり】日英字幕"},
                        {"urls", "RJ"}
                    }
                );
            }
        }

        private static HttpLoader GetHttpLoaderMock(string infoPageResp) {
            var httpLoaderMock = new Mock<HttpLoader>();
            httpLoaderMock.Setup(h => h.GetPage(It.IsAny<string>())).Returns(infoPageResp);
            return httpLoaderMock.Object;
        }

        private static string InfoPageFromResources(string name) {
            return File.ReadAllText(Path.Combine("Resources", string.Format("InfoPage-{0}.txt", name)));
        }

        private static IEnumerable<KeyValuePair<int, string>> LoadUrlsFromResources(string name) {
            var lines = File.ReadAllLines(Path.Combine("Resources", string.Format("Urls-{0}.txt", name)));
            var urls = lines
                .Select(l => l.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries))
                .Select(a => new KeyValuePair<int, string>(Convert.ToInt32(a[0]), a[1]))
                .ToArray();

            return urls;
        }
    }
}