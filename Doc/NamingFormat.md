# Simple.TV Downloader - Naming Formats

Downloaded videos are saved to your filesystem using a flexible 
format.  For example, a show might be saved as

    D:\Downloader\NCIS\Season 03\NCIS - S03E10 - Probie.mp4

Customizing how this works involves the use of tokens, such as `{EpisodeNumber}`, which 
will be replaced with the actual value from the episode.  A list of valid tokens is below.

There are three parts that determine where files are saved: `[Base Folder]\[Subfolder]\[Filename]` 


## Base Folder

The base folder defaults to the current working directory.  This can be changed 
by using the `-d` parameter.

    -d N:\Recordings


## Subfolder

The default folder where the downloader will save files is

    \{ShowName}\Season {SeasonNumber00}\
 
In the example above, this results in
 
    \NCIS\Season 03\
 
This can be customized by using the `-r` flag.  If you wanted for the folder to
show `Regular Season 03` instead of `Season 03`, you could use this command line flag:
 
     -r "{ShowName}\Regular Season {SeasonNumber00}"


## Filename

The default filename that files have is
 
    {ShowName} - S{SeasonNumber00}E{EpisodeNumber00} - {EpisodeName}.mp4

In the example aboe, this results in

    NCIS - S03E10 - Probie.mp4
 
This can be customized by using the `-n` flag.  If you don't 
want the episode name in the filename, you could do this:

    -n {ShowName} - S{SeasonNumber00}E{EpisodeNumber00}` 


# Tokens

There are several tokens that can be used for naming files and folders.  As seen above, when 
defining a format, the tokens should be enlosed in curly braces.  

- ShowName
- SeasonNumber   
- EpisodeNumber  
- SeasonNumber00 
- EpisodeNumber00
- EpisodeName  
- DateTime      
- ChannelNumber  

For the tokens ending in `00`, the `00` indicates that two digits will always be used,
with leading zeros added when necessary.  For example, if the value is `8`, then `08` will be used.