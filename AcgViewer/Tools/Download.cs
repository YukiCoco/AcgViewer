using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AcgViewer.Tools
{
    public class Download
    {

        #region 为WebClient设置超时
        //public class WebClientWithTimeout : WebClient
        //{
        //    protected override WebRequest GetWebRequest(Uri address)
        //    {
        //        WebRequest wr = base.GetWebRequest(address);
        //        wr.Timeout = 5000; // timeout in milliseconds (ms)
        //        return wr;
        //    }
        //}
        #endregion
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="savePath">保存路径</param>
        public static async Task DownloadFile(string url,string savePath)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    await client.DownloadFileTaskAsync(url, savePath);
                }
                catch
                {
                    throw new Exception("下载错误");
                }
                //client.DownloadFile(url, savePath);
            }
            CommonData.CurrentPreImgDownloadCount --;
        }

        public class Img
        {
            public string sourseUrl;
            public string saveDirectory;
            public string imgId;
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="state"></param>
        public static void DownloadImg(object state)
        {
            Img img = (Img)state;
            using (WebClient client = new WebClient())
            {
                failToDownload:
                try
                {
                    Console.WriteLine("正在下载：" + img.sourseUrl);
                    client.DownloadFile(img.sourseUrl, img.saveDirectory + "/" + img.imgId + Path.GetExtension(img.sourseUrl));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadMsg.dataLib.DownloadProgressBarValue++;
                    });
                }
                catch
                {
                    //下载失败后
                    goto failToDownload;
                }
            }
        }
    }
}
