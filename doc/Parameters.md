# Simple.TV Downloader - Command Line Parameters

There are several parameters you can use to change how the Downloader behaves.  To see
a complete list of parameters available, run the downloader without any parameters.

## Authentication

The `-u` or `-username` and `-p` or `-password` parameters are how you login
to your SimpleTV DVR.  If your username or password have special characters,
you might need to enclose them in quotation marks, like this:

    -p "MyP@ssword!"

## DownloadFolder

The `-f` or `-downloadFolder` parameter can be used to specify which folder
to place downloaded recordings in.  If this is not specified, the current
folder will be used.

## Include / Exclude Shows

The `-i` or `-includeShows` parameter will filter out all shows that do not
match the pattern.  For example, if you have these shows:

- Friends
- Friday Night Fever
- Days of our Lives

And you call the Downloader with `-i Fri*`, then two shows will be downloaded:
Friends and Friday Night Fever.

The `-x` or `-excludeShows` parameter will filter out all shows that match
the pattern.  If you have the shows listed above, and you call the Downloader
with `-x Fri*`, then one show will be downloaded: Days of our Lives.

## Include / Exclude servers

The `-s` or `-includeServers` parameter will filter out all shows on servers that do not
match the pattern.  For example, if you have these servers:

- Family Room
- Master Bedroom
- Basement

And you call the Downloader with `-s *oom`, then shows will be downloaded
from two servers:
Family Room and Master Bedroom.

The `-e` or `-excludeServers` parameter will filter out all shows on servers that match
the pattern.  If you have the servers listed above, and you call the Downloader
with `-e *oom`, then show will only be downloaded from one server: Basement.

## Logging

The `-l` or `-logHttpCalls` flag will create a log file in the current directory
that will aid in debugging problems with the downloader.

## Folder and File Name Format

The `-t` or `-folderformat` and `-n` or `-filenameformat` can
customize the folder names and filenames of the downloaded episodes.  For more
details, see the [NamingFormat documentation](doc/NamingFormat.md).

## Reboot

The `-r` or `-reboot` is used to reboot the SimpleTV DVR.  This can be used
in combination with the `-s` and `-e` parameters.

If your DVR is unreachable by the [simple.tv website](http://my.simple.tv), then
this command will not be able to reboot your DVR.
