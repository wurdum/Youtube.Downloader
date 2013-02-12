using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class UrlNormalizationTests
    {
        [Test]
        [TestCaseSource("TestCases")]
        public string IsUrlNormalized(string input) {
            return UrlHelpers.NormalizeYoutubeUrl(input);
        }

        public static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData("www.youtube.com/watch?v=tr3dFSzh1yU").Returns("http://youtube.com/watch?v=tr3dFSzh1yU");
                yield return new TestCaseData("youtube.com/watch?v=tr3dFSzh1yU").Returns("http://youtube.com/watch?v=tr3dFSzh1yU");
                yield return new TestCaseData("https://www.youtube.com/watch?v=tr3dFSzh1yU").Returns("http://youtube.com/watch?v=tr3dFSzh1yU");
                yield return new TestCaseData("http://youtu.be/tr3dFSzh1yU").Returns("http://youtube.com/watch?v=tr3dFSzh1yU");
                yield return new TestCaseData("http://www.youtube.com/v/tr3dFSzh1yU").Returns("http://youtube.com/watch?v=tr3dFSzh1yU");
                yield return new TestCaseData("url").Throws(typeof(ArgumentException));
                yield return new TestCaseData("http://notyoutube.com").Throws(typeof(ArgumentException));
                yield return new TestCaseData("http://notyoutube.com").Throws(typeof(ArgumentException));
            }
        }
    }
}