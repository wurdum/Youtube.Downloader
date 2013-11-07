Youtube.Downloader
==================

Simple console app that allows you download videos from www.youtube.com

Usage:
------

```
Examples:
Youtube.Downloader.Console.exe http://www.youtube.com/watch?v=xxxxxxxxx http://www.youtube.com/watch?v=yyyyyyyyyy - download
 two videos
Youtube.Downloader.Console.exe -f http://www.youtube.com/watch?v=xxxxxxxxx - just show list of available video formats
Youtube.Downloader.Console.exe -v http://www.youtube.com/watch?v=xxxxxxxxx - download video showing debug info
Youtube.Downloader.Console.exe -h - show help

Options:
  -f, --formatsonly          Just show info about available video formats
  -v, --verbose              Show debug info
  -h, --help                 Show help
  [ ... ]                    List of videos urls from www.youtube.com separated by spaces
```