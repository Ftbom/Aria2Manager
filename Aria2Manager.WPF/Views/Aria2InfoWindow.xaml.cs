using MahApps.Metro.Controls;
using System.Collections.Generic;

namespace Aria2Manager.Views
{
    /// <summary>
    /// Aria2InfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Aria2InfoWindow : MetroWindow
    {
        public Aria2InfoWindow(string version, IEnumerable<string> features, string versionLabel, string featuresLabel)
        {
            InitializeComponent();
            DataContext = new
            {
                VersionLabel = versionLabel,
                Version = version,
                FeaturesLabel = featuresLabel,
                Features = features
            };
        }
    }
}
