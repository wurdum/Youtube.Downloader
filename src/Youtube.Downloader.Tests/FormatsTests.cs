﻿using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class FormatsTests
    {
        [Test, TestCaseSource("ResolutionCreationTestCases")]
        public string ResolutionCreationTest(string resStr) {
            return new Resolution(resStr).ToString();
        }

        [Test, TestCaseSource("ResolutionsTestCases")]
        public int ResolutionComparingTest(Resolution res1, Resolution res2) {
            return res1.CompareTo(res2);
        }

        [Test, TestCaseSource("FormatsTestCases")]
        public IEnumerable<Format> FormatsParsingTest(IEnumerable<KeyValuePair<int, string>> videosUrls) {
            return new Formats("x", videosUrls);
        }

        [Test, TestCaseSource("FormatsGetBestTestCases")]
        public Format GetBestFormatTest(Formats formats, bool takeSpecific) {
            return formats.GetBest(takeSpecific);
        }

        private static IEnumerable<TestCaseData> FormatsGetBestTestCases {
            get {
                yield return new TestCaseData(new Formats("x", new List<KeyValuePair<int, string>> {
                    new KeyValuePair<int, string>(5, ""),
                    new KeyValuePair<int, string>(6, "yyy"),
                    new KeyValuePair<int, string>(13, "xxx"),
                }), false).Returns(new Format { Tag = 5, Resolution = new Resolution("240x400"), IsSpecific = false, DownloadUrl = "" });
                yield return new TestCaseData(new Formats("x", new List<KeyValuePair<int, string>> {
                    new KeyValuePair<int, string>(134, ""),
                    new KeyValuePair<int, string>(135, "yyy"),
                    new KeyValuePair<int, string>(136, "xxx"),
                }), false).Returns(new Format { Tag = 136, Extention = "mp4", Resolution = new Resolution("720p"), IsSpecific = true, DownloadUrl = "xxx" });
            }
        }

        private static IEnumerable<TestCaseData> FormatsTestCases {
            get {
                yield return new TestCaseData(new List<KeyValuePair<int, string>> {
                    new KeyValuePair<int, string>(5, ""),
                    new KeyValuePair<int, string>(6, "yyy"),
                    new KeyValuePair<int, string>(13, "xxx"),
                }).Returns(new List<Format> {
                    new Format {Tag = 5, Resolution = new Resolution("240x400"), IsSpecific = false, DownloadUrl = ""},
                    new Format {Tag = 6, Resolution = new Resolution("???"), IsSpecific = false, DownloadUrl = "yyy"},
                    new Format {Tag = 13, Extention = "3gp", Resolution = new Resolution("???"), IsSpecific = false, DownloadUrl = "xxx"}
                });
                yield return new TestCaseData(new List<KeyValuePair<int, string>> {
                    new KeyValuePair<int, string>(134, ""),
                    new KeyValuePair<int, string>(135, "yyy"),
                    new KeyValuePair<int, string>(136, "xxx"),
                }).Returns(new List<Format> {
                    new Format {Tag = 134, Extention = "mp4", Resolution = new Resolution("360p"), IsSpecific = true, DownloadUrl = ""},
                    new Format {Tag = 135, Extention = "mp4", Resolution = new Resolution("480p"), IsSpecific = true, DownloadUrl = "yyy"},
                    new Format {Tag = 136, Extention = "mp4", Resolution = new Resolution("720p"), IsSpecific = true, DownloadUrl = "xxx"}
                });
            }
        }

        private static IEnumerable<TestCaseData> ResolutionsTestCases {
            get {
                yield return new TestCaseData(new Resolution("???"), new Resolution("???")).Returns(0);
                yield return new TestCaseData(new Resolution("240p"), new Resolution("128k")).Returns(1);
                yield return new TestCaseData(new Resolution("1080p"), new Resolution("1080p")).Returns(0);
                yield return new TestCaseData(new Resolution("720p"), new Resolution("1080p")).Returns(-1);
                yield return new TestCaseData(new Resolution("720p"), new Resolution("1080x1920")).Returns(-1);
                yield return new TestCaseData(new Resolution("720p"), new Resolution("480x854")).Returns(1);
            }
        }

        private static IEnumerable<TestCaseData> ResolutionCreationTestCases {
            get {
                yield return new TestCaseData("??").Throws(typeof(ArgumentException));
                yield return new TestCaseData("xx").Throws(typeof(ArgumentException));
                yield return new TestCaseData("123m324").Throws(typeof(ArgumentException));
                yield return new TestCaseData("123c").Throws(typeof(ArgumentException));
                yield return new TestCaseData("123p").Returns("123p");
                yield return new TestCaseData("123k").Returns("123k");
                yield return new TestCaseData("123x324").Returns("123x324");
            }
        }
    }
}