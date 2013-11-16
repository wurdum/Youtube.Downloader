using System;

namespace Youtube.Downloader.Exceptions
{
    public class FormatNotFoundException : Exception
    {
        public FormatNotFoundException() {}
        public FormatNotFoundException(string message) : base(message) {}
    }
}