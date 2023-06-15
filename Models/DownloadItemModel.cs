using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aria2Manager.Models
{
    public class DownloadItemModel
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public int Progress { get; set; }
        public string ETA { get; set; }
        public string Status { get; set; }
        public string UploadSpeed { get; set; }
        public string DownloadSpeed { get; set; }
        public string Uploaded { get; set; }
        public string Downloaded { get; set; }
        public double Ratio { get; set; }
        public int Connections { get; set; }
        public int Seeds { get; set; }
    }
}
