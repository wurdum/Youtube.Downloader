using System;

namespace Youtube.Downloader.Exceptions
{
    public class FormatsLoadingException : Exception
    {
        public FormatsLoadingException(string message) : base(message) {}
        public FormatsLoadingException(string message, Exception innerException) : base(message, innerException) {}
    }
}