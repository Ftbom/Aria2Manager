namespace Aria2Manager.Core.Models
{
    public class NetSpeedData
    {
        public DateTime Timestamp { get; set; }
        public long DownloadSpeed { get; set; }
        public long UploadSpeed { get; set; }
        public NetSpeedData(DateTime timestamp, long downloadSpeedBytes, long uploadSpeedBytes)
        {
            Timestamp = timestamp;
            DownloadSpeed = downloadSpeedBytes;
            UploadSpeed = uploadSpeedBytes;
        }
        public NetSpeedData(long downloadSpeedBytes, long uploadSpeedBytes)
            : this(DateTime.Now, downloadSpeedBytes, uploadSpeedBytes) { }
    }
}
