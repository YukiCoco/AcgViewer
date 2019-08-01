using AcgViewer.Tools;
using CSharpKonachan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

namespace AcgViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public class Site
        {
            private string siteName;
            private string siteUrl;

            public string SiteName { get => siteName; set => siteName = value; }
            public string SiteUrl { get => siteUrl; set => siteUrl = value; }
        }
        public List<Site> sites = new List<Site>();
        #region 属性
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
            private ObservableCollection<ImgShow> _imgsItemSourse = new ObservableCollection<ImgShow>(); //表格对象实例化

            private string _siteName;
            private bool _isNotSearching = true;
            private Visibility _isLoadingProgressBar = Visibility.Collapsed;
            private string _searchTextBoxText;
            #endregion
            public ObservableCollection<ImgShow> ImgsItemSourse
            {
                get { return _imgsItemSourse; }
                set
                {
                    _imgsItemSourse = value;
                    //RaisePropertyChanged("ImgsItemSourse");
                }
            }
            public string SiteName
            {
                get { return _siteName; }
                set
                {
                    _siteName = value + " \\";
                    RaisePropertyChanged("SiteName");
                }
            }
            public bool IsNotSearching
            {
                get { return _isNotSearching; }
                set
                {
                    _isNotSearching = value;
                    RaisePropertyChanged("IsNotSearching");
                }
            }
            public Visibility IsLoadingProgressBar
            {
                get { return _isLoadingProgressBar; }
                set
                {
                    _isLoadingProgressBar = value;
                    RaisePropertyChanged("IsLoadingProgressBar");
                }
            }
            public string SearchTextBoxText
            {
                get { return _searchTextBoxText; }
                set
                {
                    _searchTextBoxText = value;
                    RaisePropertyChanged("SearchTextBoxText");
                }
            }

        }
        public static Tools.Booru booru;
        private Timer pageButtonTimer;
        #endregion
        public static DataLib dataLib = new DataLib();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = dataLib;
            sites.Add(new Site { SiteName = "KonachanCom", SiteUrl = "https://konachan.com/" });
            sites.Add(new Site { SiteName = "konachanNet", SiteUrl = "https://konachan.net/" });
            sites.Add(new Site { SiteName = "Yandere", SiteUrl = "https://yande.re/" });
            sites.Add(new Site { SiteName = "Lolibooru", SiteUrl = "https://lolibooru.moe/" });
            sites.Add(new Site { SiteName = "Gelbooru", SiteUrl = "https://gelbooru.com/" });
            SitesListBox.ItemsSource = sites;
            dataLib.SiteName = sites[0].SiteName;
            booru = new Tools.Booru(sites[0].SiteUrl, false);
            booru.InitiallyImgs();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            //dataLib.ImgsItemSourse.Add(new ImgShow(@"https://konachan.com/image/46f7c99b9bac52b84102ab6d369260bd/Konachan.com%20-%20257666%20blonde_hair%20breasts%20dress%20food%20game_cg%20glasses%20green_eyes%20green_hair%20hiyajou_maho%20huke%20long_hair%20pink_hair%20short_hair%20steins%3Bgate%20twintails.png",233));
            //dataLib.ImgsItemSourse.Add(new ImgShow(@"F:\AnimeImgs\hozumi\387340.jpg"));
            //imgsListBox.Items.Refresh();
            this.Close();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void ImgListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool isAtButtom = false;
            // get the vertical scroll position
            double dVer = e.VerticalOffset;
            //get the vertical size of the scrollable content area
            double dViewport = e.ViewportHeight;
            //get the vertical size of the visible content area
            double dExtent = e.ExtentHeight;
            if (dVer != 0)
            {
                if (dVer + dViewport == dExtent)
                {
                    isAtButtom = true;
                }
                else
                {
                    isAtButtom = false;
                }
            }
            else
            {
                isAtButtom = false;
            }
            if (isAtButtom)
            {

            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                dataLib.ImgsItemSourse.Clear();
                List<string> tags = new List<string>();
                tags.Add(SearchTextBox.Text);
                CommonData.PostCount = booru.booruClient.GetPageCount(tags);
                booru.SearchPost(CommonData.CurrentPage, SearchTextBox.Text);
            }
        }
        private void PopupBox_MouseMove(object sender, MouseEventArgs e)
        {
            NextPageButton.Visibility = Visibility.Visible;
            LastPageButton.Visibility = Visibility.Visible;
            pageButtonTimer = new Timer(new TimerCallback((object state) =>
            {
                pageButtonTimer.Change(-1, 0);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(!NextPageButton.IsMouseOver && !LastPageButton.IsMouseOver && !PopupBoxButton.IsMouseOver)
                    {
                        NextPageButton.Visibility = Visibility.Hidden;
                        LastPageButton.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        pageButtonTimer.Change(1000, 0);
                    }
                });
            }));
            pageButtonTimer.Change(1000, 0);
        }
        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommonData.CurrentPage < CommonData.PostCount & CommonData.CurrentPage != 1)
            {
                CommonData.CurrentPage--;
                dataLib.ImgsItemSourse.Clear();
                booru.SearchPost(CommonData.CurrentPage, SearchTextBox.Text);
            }
        }
        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            CommonData.CurrentPage++;
            if (CommonData.CurrentPage < CommonData.PostCount)
            {
                dataLib.ImgsItemSourse.Clear();
                booru.SearchPost(CommonData.CurrentPage, SearchTextBox.Text);
            }
        }
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MenuToggleButton.IsChecked = false;
            dataLib.SiteName = sites[SitesListBox.SelectedIndex].SiteName;
            dataLib.ImgsItemSourse.Clear();
            booru = new Tools.Booru(sites[SitesListBox.SelectedIndex].SiteUrl, false);
            booru.InitiallyImgs();
        }
    }
}
