using System;

namespace Youtube.Downloader.Exceptions
{
    public class YoutubeParseException : Exception
    {
        public YoutubeParseException(string message) : base(message) {}
        public YoutubeParseException(string message, Exception innerException) : base(message, innerException) {}
    }
}