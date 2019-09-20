using CSharpKonachan.Models;
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

namespace AcgViewer.Tools
{
    public class Download
    {
        #region 属性
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
                //if (_currentImgDownloadCount == 0)
                //    DownloadMsg.dataLib.DownloadButtonIsEnabled = true;

            }
        }
        //总任务数
        private static int TotalImgDownloadCount { get; set; }

        /// <summary>
        /// 重写超时
        /// </summary>
        public class WebClientWithTimeout : WebClient
        {
            private int timeout = 10000;
            public int Timeout { get => timeout; set => timeout = value; }

            /// <summary>
            /// 重写带上超时设定
            /// </summary>
            /// <param name="address"></param>
            /// <returns></returns>
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = Timeout; // timeout in milliseconds (ms)
                return wr;
            }
        }
        #endregion
        ///// <summary>
        ///// 下载文件
        ///// </summary>
        ///// <param name="url">Url</param>
        ///// <param name="savePath">保存路径</param>
        //public static async Task DownloadFile(string url, string savePath)
        //{
        //    using (WebClientWithTimeout webClient = new WebClientWithTimeout())
        //    {
        //        //await webClient.DownloadFileTaskAsync(url, savePath);
        //        await Task.Run(() =>
        //        {
        //            bool isTimeout = false;
        //            do
        //            {
        //                try
        //                {
        //                    webClient.DownloadFile(url, savePath);
        //                    isTimeout = false;
        //                }
        //                catch (WebException we)
        //                {
        //                    switch (we.Status)
        //                    {
        //                            //超时
        //                            case WebExceptionStatus.Timeout:
        //                            Console.WriteLine("下载超时");
        //                            isTimeout = true;
        //                            break;
        //                        default:
        //                                //throw new Exception("网络错误");
        //                                //downloadCompletedOrError();
        //                            break;
        //                    }
        //                }
        //            } while (isTimeout);
        //        });
        //    }
        //    //client.DownloadFile(url, savePath);
        //}
        //CommonData.CurrentPreImgDownloadCount --;


        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="savePath"></param>
        public async void DownloadImgs(List<Post> posts, string savePath)
        {
            lock (o)
            {
                TotalImgDownloadCount += posts.Count;
            }
            foreach (var item in posts)
            {
                await Task.Run(() =>
                {
                    while (true)
                    {
                        //同时下载3个图片
                        if (CurrentImgDownloadCount <= 3)
                        {
                            string filePath = savePath + "\\" + item.id + Path.GetExtension(item.file_url);
                            lock (o)
                            {
                                CurrentImgDownloadCount++;
                                Debug.WriteLine("增加后" + CurrentImgDownloadCount);
                            }
                            //开启下载线程
                            DownloadFile(item.file_url, filePath,true, 15000);
                            #region 为异步下载设置超时
                            //webClient.DownloadFileAsync(new Uri(item.file_url), filePath);

                            //System.Timers.ElapsedEventHandler handler = null;
                            //handler = ((sender, args)
                            //=>
                            //{
                            //    //超时执行
                            //    Console.WriteLine(item.id + "下载超时");
                            //   webClient.CancelAsync();
                            //    webClient.Dispose();
                            //    Thread.Sleep(2000);
                            //    File.Delete(filePath);
                            //    webClient = new WebClient(); //WebClient一次连接一个服务器，故如此
                            //    webClient.DownloadFileAsync(new Uri(item.file_url), filePath);
                            //    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                            //});
                            //aTimer.Elapsed += handler;
                            //aTimer.Interval = 10000; //10秒钟
                            //aTimer.Enabled = true;
                            #endregion
                            //webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                });
            }
        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// /// <param name="isCounted">是否计数</param>
        public static async Task DownloadFile(string url, string filePath, bool isCounted, int timeout)
        {
            await Task.Run(() =>
            {
                bool isRetry = false;
                WebClientWithTimeout webClient = new WebClientWithTimeout();
                do
                {
                    //超时重载
                    if (isRetry)
                    {
                        lock (o)
                        {
                            EventHandler eventHandler = new EventHandler((object o, EventArgs e) =>
                            {
                                File.Delete(filePath);
                            });
                            webClient.Dispose();
                            webClient.Disposed += eventHandler;
                            webClient = new WebClientWithTimeout();
                        }
                    }
                    try
                    {
                        webClient.Timeout = timeout;
                        webClient.DownloadFile(new Uri(url), filePath);
                        if (isCounted)
                            DownloadCompletedOrError();//下载图片计数
                        isRetry = false;
                    }
                    catch (WebException we)
                    {
                        switch (we.Status)
                        {
                            //超时
                            case WebExceptionStatus.Timeout:
                                Debug.WriteLine("下载超时");
                                isRetry = true;
                                break;
                            case WebExceptionStatus.RequestCanceled:
                                Debug.WriteLine("请求被取消");
                                isRetry = true;
                                break;
                            default:
                                if (isCounted)
                                    DownloadCompletedOrError();
                                Debug.WriteLine("下载错误："+ we.Status);
                                return;
                                //throw new Exception("网络错误");
                        }
                    }
                }
                while (isRetry);
            });
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        private static void DownloadCompletedOrError()
        {
            lock (o)
            {
                CurrentImgDownloadCount--;
                TotalImgDownloadCount--;
                Debug.WriteLine("减少后" + CurrentImgDownloadCount);
                //下载完毕
                if (TotalImgDownloadCount == 0)
                {
                    DownloadMsg.dataLib.DownloadButtonIsEnabled = true;
                }
            }
            //更新进度条
            DownloadMsg.dataLib.DownloadProgressBarValue++;
        }
    }
}
