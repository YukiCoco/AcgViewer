using CSharpKonachan;
using CSharpKonachan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AcgViewer.Tools
{
    public class Booru
    {
        public BooruClient booruClient;

        private string userSite;
        private bool isNsfw;
        public Booru(string Site, bool nsfw)
        {
            userSite = Site;
            isNsfw = nsfw;
            booruClient = new BooruClient(Site, nsfw);
        }
        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async void InitiallyImgs()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.dataLib.IsLoadingProgressBar = Visibility.Visible;
            }));

            List<Post> posts = new List<Post>();
            await Task.Run(() =>
            {
                posts = booruClient.GetPost();
            });
            
            CommonData.CurrentPreImgDownloadCount = posts.Count;
            for (int i = 0; i < posts.Count; i++)
            {
                 await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ImgShow imgShow = new ImgShow(posts[i].preview_url, posts[i].id, posts[i].preview_width, posts[i].preview_height);
                    }));
                });
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.dataLib.IsLoadingProgressBar = Visibility.Collapsed;
            }));
        }
        public async void SearchPost(int page,string tags)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.dataLib.IsLoadingProgressBar = Visibility.Visible;
            }));
            List<string> tagsList = new List<string>();
            foreach (var item in tags.Split(' '))
            {
                tagsList.Add(item);
            }
            List<Post> posts = new List<Post>();
            await Task.Run(() =>
            {
                posts = booruClient.GetPost(page, tagsList);
                //CommonData.PostCount = booruClient.GetPageCount(tagsList);
                while (true)
                {
                    if (CommonData.CurrentPreImgDownloadCount == 0)
                    {
                        CommonData.CurrentPreImgDownloadCount = posts.Count;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                }
            });
            
            for (int i = 0; i < posts.Count; i++)
            {
                await Task.Run(() =>
                {
                    
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ImgShow imgShow = new ImgShow(posts[i].preview_url, posts[i].id, posts[i].preview_width, posts[i].preview_height);
                    }));
                });
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.dataLib.IsLoadingProgressBar = Visibility.Collapsed;
            }));
        }
    }
}
