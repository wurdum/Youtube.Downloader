using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Youtube.Downloader.Http
{
    public class HttpLoader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly CookieContainer _coockieContainer;

        public HttpLoader(bool saveCookie) {
            if (saveCookie)
                _coockieContainer = new CookieContainer();
        }

        public HttpLoader() : this(false) { }

        public CookieContainer Cookie {
            get { return _coockieContainer; }
        }

        public virtual string GetPage(string url) {
            return GetPageAsync(url).Result;
        }

        public virtual string GetPage(string url, Encoding encoding) {
            return GetPageAsync(url, encoding).Result;
        }

        public virtual Task<string> GetPageAsync(string url) {
            return GetPageAsync(url, Encoding.UTF8);
        }

        public virtual async Task<string> GetPageAsync(string url, Encoding encoding) {
            Logger.Debug("requesting {0}page '{1}'", _coockieContainer == null ? "" : "with saving cookie ", url);
            var request = CreateStatefulRequest(url);
            request.KeepAlive = false;
            var response = await request.GetResponseAsync();
            
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new NullReferenceException("Response stream for '" + request.RequestUri + "' is empty");

            var page = new StringBuilder();
            using (var reader = new StreamReader(responseStream, encoding)) {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    page.Append(line);
            }
            Logger.Debug("read from '{0}' {1} bytes", url, encoding.GetBytes(page.ToString()).Length);
            return page.ToString();
        }

        public HttpWebRequest CreateStatefulRequest(string url) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _coockieContainer;
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20100101 Firefox/10.0 (Chrome)";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers[HttpRequestHeader.AcceptCharset] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Headers[HttpRequestHeader.AcceptLanguage] = "en-us,en;q=0.5";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }
}