using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class UrlComposerTests
    {
        [Test, TestCaseSource("ParseIdSource")]
        public string ParseIdTests(string url) {
            return UrlsComposer.ParseId(url);
        }

        [Test]
        public void MultipleThereadsTest() {
            var testCaseDatas = new List<TestCaseData>();
            for (var i = 0; i < 10; i++)
                testCaseDatas.AddRange(ParseIdSource.Cast<TestCaseData>().ToArray());

            var inputs = testCaseDatas.Select(tc => (string)tc.Arguments[0]).ToArray();
            var outputs = testCaseDatas.Select(tc => (string)tc.Result).ToArray();

            var results = new string[inputs.Length];

            var threads = new Thread[inputs.Length];
            var semaphore = new Semaphore(0, inputs.Length);
            for (var i = 0; i < inputs.Length; i++) {
                var index = i;
                threads[index] = new Thread(() => {
                    Console.WriteLine("waiting " + index);
                    semaphore.WaitOne();
                    results[index] = UrlsComposer.ParseId(inputs[index]);
                    Console.WriteLine("done " + index);
                });

                threads[index].Start();
            }

            semaphore.Release(inputs.Length);

            for (var i = 0; i < inputs.Length; i++)
                threads[i].Join();

            for (var i = 0; i < inputs.Length; i++)
                Assert.AreEqual(outputs[i], results[i]);
        }

        private static IEnumerable ParseIdSource {
            get {
                yield return new TestCaseData("http://www.youtube.com/watch?v=UiyDmqO59QE").Returns("UiyDmqO59QE");
                yield return new TestCaseData("https://www.youtube.com/watch?v=UiyDmqO59QE").Returns("UiyDmqO59QE");
                yield return new TestCaseData("www.youtube.com/watch?v=UiyDmqO59QE").Returns("UiyDmqO59QE");
                yield return new TestCaseData("UiyDmqO59QE").Returns("UiyDmqO59QE");
                yield return new TestCaseData("http://www.youtu.be/MzPRTRnEUfs").Returns("MzPRTRnEUfs");
                yield return new TestCaseData("http://www.youtube.com/watch?v=Ox11tFjBb2A&list=PLIBsKu92G5opBjw0RJQsNmiCMqka9C38i&index=1")
                    .Returns("Ox11tFjBb2A");
            }
        }
    }
}