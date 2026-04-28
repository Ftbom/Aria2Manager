namespace Aria2Manager.Core.Models
{
    public class FileDialogFilter
    {
        //名称、描述
        public string Name { get; set; } = string.Empty;
        //允许一个类别包含多个后缀
        public List<string> Extensions { get; set; } = new List<string>();
    }
}
