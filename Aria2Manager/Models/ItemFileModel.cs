namespace Aria2Manager.Models
{
    //下载项的文件信息
    public class ItemFileModel
    {
        public string Index; //序号
        public bool Selected { get; set; } //是否选中（下载）
        public string Name { get; set; } //名称
    }
}
