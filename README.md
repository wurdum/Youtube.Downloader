## Youtube.Downloader

Simple console app that allows you download videos from www.youtube.com

### Features:
* can download several videos simulteniously
* can continue downloading if previous session was terminated
* allows you to receive info about available videos formats

### Usage:

```
Examples:
Youtube.Downloader.Console.exe http://www.youtube.com/watch?v=xxxxxxxxx http://www.youtube.com/watch?v=yyyyyyyyyy - 
  download two videos
Youtube.Downloader.Console.exe -m -e mp4 http://www.youtube.com/watch?v=xxxxxxxxx - download video in medium quality, 
  prefer mp4
Youtube.Downloader.Console.exe -f http://www.youtube.com/watch?v=xxxxxxxxx - just show list of available video formats
Youtube.Downloader.Console.exe -p D:\ http://www.youtube.com/watch?v=xxxxxxxxx - download video and save it to D:\
Youtube.Downloader.Console.exe -h - show help

Options:
  -p, --path=VALUE           Path where to save videos. If not specified
                               saves to Desktop.
  -e, --preferextention=VALUE
                             Preferable video extention, like mp4 or webm
  -f, --formatsonly          Just show info about available video formats
  -n, --newfiles             Do not allow continue downloading. Just create
                               new files.
  -m, --mediumquality        Download videos in medium quality
  -h, --help                 Show help
  [ ... ]                    List of videos urls from www.youtube.com separated by spaces
```