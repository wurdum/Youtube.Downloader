using NUnit.Framework;
using Youtube.Downloader.Http;

namespace Youtube.Downloader.Tests
{
    [TestFixture]
    public class HttpLoaderTests
    {
        [Test]
        public void CookiesAreSavedTest() {
            var httpLoader = new HttpLoader(true);

            httpLoader.GetPage("https://www.youtube.com/");

            Assert.True(httpLoader.Cookie != null);
            Assert.True(httpLoader.Cookie.Count > 0);
        }
    }
}