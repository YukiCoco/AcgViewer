using CSharpKonachan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcgViewer.Tools
{
    public class WebDownload
    {

        private static int _currentImgDownloadCount;
        private static readonly object o = new object();
        //当前下载任务数
        public static int CurrentImgDownloadCount
        {
            get
            {
                return _currentImgDownloadCount;
            }
            set
            {
                _currentImgDownloadCount = value;
                if (_currentImgDownloadCount == 0)
                    DownloadMsg.dataLib.DownloadButtonIsEnabled = true;
                
            }
        }
        public  WebDownload()
        {
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="savePath"></param>
        public void DownloadImgs(List<Post> posts, string savePath)
        {
            foreach (var item in posts)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        //同时下载3个图片
                        if (CurrentImgDownloadCount <= 3)
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                lock (o)
                                {
                                    CurrentImgDownloadCount++;
                                }
                                webClient.DownloadFileAsync(new Uri(item.file_url), savePath + "\\" + item.id + Path.GetExtension(item.file_url));
                                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                            }
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                });
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            lock (o)
            {
                CurrentImgDownloadCount--;
                //下载完毕
                if(CurrentImgDownloadCount == 0)
                {
                    DownloadMsg.dataLib.DownloadButtonIsEnabled = true;
                }
            }
            //更新进度条
            DownloadMsg.dataLib.DownloadProgressBarValue++; 
        }
    }
}
