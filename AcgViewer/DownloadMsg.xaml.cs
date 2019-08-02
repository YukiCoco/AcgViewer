using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using CSharpKonachan.Models;

namespace AcgViewer
{
    /// <summary>
    /// DownloadMsg.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadMsg : UserControl
    {
        public static double DownloadProgressBarValue;

        public class DataLib : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged属性
            //INotifyPropertyChanged属性
            public event PropertyChangedEventHandler PropertyChanged;

            //属性更改方法
            private void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
            #region 字段
            private double _downloadProgressBarValue;
            private bool _downloadButtonIsEnabled;
            private string _downloadMsgText;
            private string _directoryText;
            private double _downloadProgressBarMaximum;
            private bool _sigleCheck;
            private bool _multiCheck;
            #endregion
            public double DownloadProgressBarValue
            {
                get { return _downloadProgressBarValue; }
                set
                {
                    _downloadProgressBarValue = value;
                    RaisePropertyChanged("DownloadProgressBarValue");
                }
            }
            public bool DownloadButtonIsEnabled
            {
                get { return _downloadButtonIsEnabled; }
                set
                {
                    _downloadButtonIsEnabled = value;
                    RaisePropertyChanged("DownloadButtonIsEnabled");
                }
            }
            public string DownloadMsgText
            {
                get { return _downloadMsgText; }
                set
                {
                    _downloadMsgText = value;
                    RaisePropertyChanged("DownloadMsgText");
                }
            }
            public string DirectoryText
            {
                get { return _directoryText; }
                set
                {
                    _directoryText = value;
                    RaisePropertyChanged("DirectoryText");
                }
            }

            public double DownloadProgressBarMaximum { get => _downloadProgressBarMaximum; set
                {
                    _downloadProgressBarMaximum = value;
                    RaisePropertyChanged("DownloadProgressBarMaximum");
                }
            }

            public bool SigleCheck { get => _sigleCheck; set
                {
                    _sigleCheck = value;
                    RaisePropertyChanged("SigleCheck");
                } }
            public bool MultiCheck { get => _multiCheck; set
                {
                    _multiCheck = value;
                    RaisePropertyChanged("MultiCheck");
                }

            }
        }

        public static DataLib dataLib = new DataLib();
        public DownloadMsg()
        {
            InitializeComponent();
            this.DataContext = dataLib;
            dataLib.DownloadButtonIsEnabled = true;
            dataLib.MultiCheck = true;
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            //DirectoryTextBox.IsEnabled = false;
            dataLib.DownloadButtonIsEnabled = false;
            dataLib.DownloadProgressBarValue = 0;
            List<string> tags = new List<string>();
            tags.Add(MainWindow.dataLib.SearchTextBoxText.Replace(' ','_'));

            //多页下载
            if(dataLib.MultiCheck == true)
            {
                List<List<Post>> postArray = new List<List<Post>>();
                await Task.Run(() =>
                {
                    //开始循环下载
                    for (int i = 1; i <= CommonData.PostCount; i++)
                    {
                        dataLib.DownloadMsgText = String.Format("共查询到{0}页，正在读取第{1}页", CommonData.PostCount, i);
                        List<Post> posts = new List<Post>();
                        posts = MainWindow.booru.booruClient.GetPost(i, tags);
                        postArray.Add(posts);
                    }
                    //计算图片总数
                    int postsSum = 0;
                    foreach (var item in postArray)
                    {
                        postsSum += item.Count;
                    }
                    dataLib.DownloadProgressBarMaximum = postsSum;
                    //开始下载
                    Download webDownload = new Download();
                    foreach (var item in postArray)
                    {
                        webDownload.DownloadImgs(item, dataLib.DirectoryText);
                    }
                    //posts = MainWindow.booru.booruClient.GetPost(tags);
                });
            }
            else if(dataLib.SigleCheck  == true)
            {
                await Task.Run(() =>
                {
                    List<Post> posts = new List<Post>();
                    posts = MainWindow.booru.booruClient.GetPost(CommonData.CurrentPage, tags);
                    dataLib.DownloadMsgText = String.Format("共查询到{0}页，正在读取第{1}页", CommonData.PostCount, CommonData.CurrentPage);
                    //图片总数
                    dataLib.DownloadProgressBarMaximum = posts.Count;
                    //开始下载
                    Download webDownload = new Download();
                    webDownload.DownloadImgs(posts, dataLib.DirectoryText);
                });
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dataLib.MultiCheck == true)
            {
                dataLib.MultiCheck = false;
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (dataLib.SigleCheck == true)
            {
                dataLib.SigleCheck = false;
            }
        }
    }
}
