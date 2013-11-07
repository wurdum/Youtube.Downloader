using System;

namespace Youtube.Downloader.Exceptions
{
    public class IdNotFoundException : Exception
    {
        public IdNotFoundException(string message) : base(message) {}
    }
}