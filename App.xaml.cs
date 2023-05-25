using System;
using System.Threading;
using System.Windows;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //需要在加载App.xaml后调用才可生效
        //可放在MainWindow的初始化函数中调用
        public void SetLanguageDictionary()
        {
            //根据系统语言环境设置语言文件
            SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString());
        }

        public void SetLanguageDictionary(String Language)
        {
            Resources.MergedDictionaries.Clear(); //清空语言资源文件
            ResourceDictionary dict = new ResourceDictionary();
            try
            {
                dict.Source = new Uri($"..\\Languages\\Strings.{Language}.xaml", UriKind.Relative);
            }
            catch
            {
                dict.Source = new Uri($"..\\Languages\\Strings.xaml", UriKind.Relative);
            }
            //导入对应的语言文件
            Resources.MergedDictionaries.Add(dict);
        }
    }
}
