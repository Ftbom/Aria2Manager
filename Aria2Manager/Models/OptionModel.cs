using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aria2Manager.Models
{
    //aria2配置项信息
    public class OptionModel : INotifyPropertyChanged
    {
        private string _value;

        public event PropertyChangedEventHandler? PropertyChanged;
        public string id { get; set; } //配置项id
        public string value //配置项值
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        public bool is_enabled { get; set; } //配置项编辑框是否启用（是否只读）
        public string? name {get; set; } //配置项名称
        public string? description { get; set; } //配置项描述

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
