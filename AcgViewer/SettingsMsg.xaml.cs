using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AcgViewer.Tools;

namespace AcgViewer
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsMsg : UserControl
    {
        public SettingsMsg()
        {
            InitializeComponent();
            ConfigFile configFile = ConfigFile.LoadOrCreateFile("config.yml");
            isNsfw.IsChecked = CommonData.isNsfw;
            if (!File.Exists("config.yml")){
                isNsfw.IsChecked = false;
            }
            //ConfigFile configFile = ConfigFile.LoadOrCreateFile("config.yml");
            //CommonData.isNsfw = bool.Parse(configFile["nsfw"]);
        }
        private void IsNsfw_Click(object sender, RoutedEventArgs e)
        {
            if (isNsfw.IsChecked == true)
            {
                CommonData.isNsfw = true;
            }
            else
            {
                CommonData.isNsfw = false;
            }
            
        }
    }
}
