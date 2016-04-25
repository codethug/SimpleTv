namespace SimpleTv.Downloader
{
    public class Configuration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        // Use newer Folder parameter instead of DownloadFolder.  
        public string DownloadFolder { get; set; }
        public string FolderFormat { get; set; }
        public string FilenameFormat { get; set; }
        public bool OverwriteExistingDownloads { get; set; }
        public string ShowIncludeFilter { get; set; }
        public string ShowExcludeFilter { get; set; }
        public string ServerIncludeFilter { get; set; }
        public string ServerExcludeFilter { get; set; }
        public bool Reboot { get; set; }
        public bool LogHttpCalls { get; set; }
    }
}
