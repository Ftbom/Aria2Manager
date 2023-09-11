using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aria2Manager.Models
{
    public class OptionModel : INotifyPropertyChanged
    {
        private string _value;

        public event PropertyChangedEventHandler? PropertyChanged;
        public string id { get; set; }
        public string value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        public bool is_enabled { get; set; }
        public string? name {get; set; }
        public string? description { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
