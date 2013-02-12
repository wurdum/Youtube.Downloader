using System;

namespace Youtube.Downloader.Exceptions
{
    public class VideoNotAvailableException : Exception
    {
        public VideoNotAvailableException(string message) : base(message) {}
    }
}