namespace Aria2Manager.Models
{
    public class DownloadItemModel
    {
        public string Gid { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public double Progress { get; set; }
        public string ETA { get; set; }
        public string Status { get; set; }
        public string UploadSpeed { get; set; }
        public string DownloadSpeed { get; set; }
        public string Uploaded { get; set; }
        public string Downloaded { get; set; }
        public double? Ratio { get; set; }
        public long Connections { get; set; }
        public long? Seeds { get; set; }
    }
}
