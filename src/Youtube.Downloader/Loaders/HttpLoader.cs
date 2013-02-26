using System.Net;
using System.Text;

namespace Youtube.Downloader.Loaders
{
    public class HttpLoader
    {
        public virtual string GetPage(string url) {
            return GetPage(url, Encoding.UTF8);
        }

        public virtual string GetPage(string url, Encoding encoding) {
            using (var client = new WebClient { Encoding = encoding })
                return client.DownloadString(url);
        } 
    }
}