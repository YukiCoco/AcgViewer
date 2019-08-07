using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace AcgViewer
{
    /// <summary>
    /// ImgShow.xaml 的交互逻辑
    /// </summary>
    public partial class ImgShow : UserControl
    {
        private double _imgWidth;
        private double _imgHight;
        private string _imgUrl;
        private int _imgID;
        public ImgShow(string imgUrl,int imgID,Double imgWidth, Double imgHight)
        {
            InitializeComponent();
            _imgWidth = imgWidth;
            _imgHight = imgHight;
            _imgUrl = imgUrl;
            _imgID = imgID;
            try
            {
                LoadPreview(imgUrl, imgID);
            }catch(Exception e)
            {
                LoadingTextBlock.Text = e.Message + "\n点击重试";
                ExcepitonLog.WriteLog(e);
                LoadingTextBlock.MouseLeftButtonUp += LoadingTextBlock_MouseLeftButtonUp;
            }
        }

        private void LoadingTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LoadPreview(_imgUrl, _imgID);
            }
            catch
            {
                MainWindow.dataLib.ImgsItemSourse.Remove(this);
            }
        }

        /// <summary>
        /// 加载预览图
        /// </summary>
        /// <param name="imgUrl">URL</param>
        /// <param name="imgID">ID</param>
        private async void LoadPreview(string imgUrl, int imgID)
        {
            Img.Width = _imgWidth;
            Img.Height = _imgHight;
            MainWindow.dataLib.ImgsItemSourse.Add(this);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\cache\\" + MainWindow.dataLib.SiteName);
            string imgPath = Directory.GetCurrentDirectory() + "\\" + string.Format("cache\\{0}\\{1}_preview{2}", MainWindow.dataLib.SiteName, imgID,System.IO.Path.GetExtension(imgUrl));

            if (File.Exists(imgPath))
            {
                File.Delete(imgPath);
            }
            await Download.DownloadFile(imgUrl, imgPath,false);
            await Task.Run(() =>
            {
                Img.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingTextBlock.Visibility = Visibility.Collapsed;
                    using (BinaryReader binReader = new BinaryReader(File.Open(imgPath, FileMode.Open)))
                    {
                        FileInfo fileInfo = new FileInfo(imgPath);
                        byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                        binReader.Close();
                        BitmapImage bi = new BitmapImage();
                        try
                        {
                            bi.BeginInit();
                            bi.StreamSource = new MemoryStream(bytes);
                            bi.EndInit();
                        }
                        catch
                        {
                            MainWindow.dataLib.ImgsItemSourse.Remove(this);
                            //throw new Exception("图片加载错误");
                        }
                        Img.Source = bi;
                    }
                }
                ));
            });
        }
        
    }
}
