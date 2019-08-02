using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpKonachan.Models;
using Newtonsoft.Json;

[assembly: CLSCompliant(true)]
namespace CSharpKonachan
{
    public class BooruClient
    {
        private string site;
        private bool isnsfw;
        public BooruClient(string userSite,bool nsfw)
        {
            site = userSite;
            isnsfw = nsfw;
        }

        /// <summary>
        /// 返回获取结果
        /// </summary>
        public class PostResult
        {
            public List<Post> Posts { get; set; }
            public Exception Exception { get; set; }
        }

        private string post = "post.json?";
        private string tag = "tag.json?";
        private string note = "note.json?";
        private string user = "user.json?";
        private string artist = "artist.json?";

        private string Limit(int? limit) { return "limit=" + limit + ""; }
        private string Page(int? page) { return "page=" + page + "&"; }
        private string Tags(List<string> tags) {
            string listTags = "";
            for (int i = 0; i < tags.Count; i++)
            {
                listTags += tags[i];
                if (i > 0)
                {
                    listTags += "+";
                }
            }
            return "tags=" + listTags;
        }
        private string Order(orderTag? order) { return "order=" + order.ToString() + "&"; }
        private string Id(int? id) { return "id=" + id + "&"; }
        private string After_id(int? after_id) { return "after_id=" + after_id + "&"; }
        private string Name(string name) { return "name=" + name + "&"; }
        private string Name_pattern(string name_pattern) { return "name_pattern=" + name_pattern + "&"; }
        private string Post_id(int? post_id) { return "post_id" + post_id + "&"; }

        public enum orderTag { date, count, name }

        class Element
        {
            public string href;
            public string title;
        }

        /// <summary>
        /// 重写WebClient 使支持TimeOut
        /// </summary>
        private class WebClientWithTimeout : WebClient
        {

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 10000; //十秒钟超时
                return w;
            }
        }

        string GetUrl(string url)
        {
            using(var x = new WebClientWithTimeout())
            {
                x.Encoding = Encoding.UTF8;
                bool isTimeout = false;
                string result = "";
                do
                {
                    try
                    {
                        result = x.DownloadString(url);
                    }
                    catch(WebException we)
                    {
                        switch (we.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                isTimeout = true;
                                break;
                            default:
                                throw we;
                        }
                    }
                } while (isTimeout);
                return result;
            }
        }

        /// <summary>
        /// 获取所有Post
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public List<Post> GetPost(List<string>  tags = null)
        {
            int pageCount = 1;
            var url = site + post;
            List<Post> posts = new List<Post>();
            //获取最大page数
            url += Page(pageCount);
            if (isnsfw == false)
            {
                url += " rating:safe";
            }
            pageCount = GetPageCount(tags);
            string html = null;
            for (int i = 1; i <= pageCount; i++)
            {
                url = site + post;
                url += Page(i);
                if (tags != null)
                {
                    url += Tags(tags);
                }
                if (isnsfw == false)
                {
                    url += " rating:safe";
                }
                html = GetUrl(url);
                posts = JsonConvert.DeserializeObject<List<Post>>(html);
            }
            return posts;
        }

        /// <summary>
        /// 获取Page数
        /// </summary>
        /// <returns></returns>
        public int GetPageCount(List<string> tags = null)
        {
            int pageCount = 1;
            var url = site + post;
            //获取最大page数
            url += Page(pageCount);
            //获取最大page数
            url += Page(pageCount);
            if (tags != null)
            {
                url += Tags(tags);
            }
            if (isnsfw == false)
            {
                url += " rating:safe";
            }
            using (WebClientWithTimeout webclient = new WebClientWithTimeout())
            {
                //伪装成Chrome
                webclient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36");
                bool isTimeout = false;
                string result = "";
                do
                {
                    try
                    {
                        result = Encoding.GetEncoding("UTF-8").GetString(webclient.DownloadData(url.Replace(".json", "")));
                    }
                    catch(WebException we)
                    {
                        switch (we.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                isTimeout = true;
                                break;
                            default:
                                throw new Exception(we.Message);
                        }
                    }
                } while (isTimeout);
                
                string patten = @"(href|rel|title)=""([^""]*)";
                List<Element> elements = new List<Element>();
                Regex.Matches(result, patten).Cast<Match>().ToList().ForEach(x =>
                {
                    if (x.Groups[1].Value == "href")
                    {
                        elements.Add(new Element { href = x.Groups[2].Value }); //添加到元素中
                    }
                    if (x.Groups[1].Value == "title")
                    {
                        elements[elements.Count - 1].title = x.Groups[2].Value;
                    }
                });
                Element lastPageElement = elements.Find(x => x.title == "Last Page");
                //别问我这是什么逻辑，当时就是这样翻源码的QwQ
                //能用就好（有时间会改改
                if (lastPageElement != null)
                {
                    int i = lastPageElement.href.IndexOf("=");//找=的位置
                    int j = lastPageElement.href.IndexOf("&");//找&的位置
                    pageCount = Convert.ToInt32(lastPageElement.href.Substring(i + 1).Substring(0, j - i - 1));
                    //Console.WriteLine("共找到{0}页内容，正在解析...", pageCount);
                }
            }
            return pageCount;
        }

        /// <summary>
        /// 获取Post
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public List<Post> GetPost(int? page = null, List<string> tags = null)
        {
            var url = site + post;
            if (page != null)
            {
                url += Page(page);
            }
            if (tags != null)
            {
                url += Tags(tags);
            }
            if(isnsfw == false)
            {
                url += " rating:safe";
            }
            string html = GetUrl(url);
            List<Post> posts = new List<Post>();
            posts = JsonConvert.DeserializeObject<List<Post>>(html);
            //foreach (var item in JsonConvert.DeserializeObject<Post[]>(html))
            //{
            //    posts.Add(item);
            //}
            return posts;
        }

        /// <summary>
        /// 获取初始页面（无TAG）
        /// </summary>
        /// <returns></returns>
        public List<Post> GetPost()
        {
            List<Post> posts = new List<Post>();
            var url = site + post;
            url += "tags=";
            if (isnsfw == false)
            {
                url += " rating:safe";
            }
            string html = GetUrl(url);
            posts = JsonConvert.DeserializeObject<List<Post>>(html);
            return posts;
        }

        public Tag[] GetTag(int? id = null, int? after_id = null, int? limit = null, int? page = null, orderTag? order = null)

        {
            var url = site+ tag;
            if (id != null)
            {
                url += Id(id);
            }
            else if (after_id != null)
            {
                url += After_id(after_id);
            }
            else if (limit != null)
            {
                url += Limit(limit);
            }
            else if (page != null)
            {
                url += Page(page);
            }
            else if (order != null)
            {
                url += Order(order);
            }

            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<Tag[]>(html);
        }
        public Tag[] GetTag(string name = null, string name_pattern = null, int? limit = null, int? page = null, orderTag? order = null)

        {
            var url = site + tag;
            if (name != null)
            {
                url += Name(name);
            }
            else if (name_pattern != null)
            {
                url += Name_pattern(name_pattern);
            }
            else if (limit != null)
            {
                url += Limit(limit);
            }
            else if (page != null)
            {
                url += Page(page);
            }
            else if (order != null)
            {
                url += Order(order);
            }
            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<Tag[]>(html);
        }

        public Artist[] GetArtist(string name = null, orderTag? order = null, int? page = null)
        {
            var url = site + artist;
            if (name != null)
            {
                url += Name(name);
            }
            else if (order != null)
            {
                url += Order(order);
            }
            else if (page != null)
            {
                url += Page(page);
            }
            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<Artist[]>(html);
        }

        public Note[] GetNote(int? post_id = null)
        {
            var url = site + note;
            if (post_id != null)
            {
                url += Post_id(post_id);
            }
            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<Note[]>(html);
        }

        public User[] GetUser(int? id= null)
        {
            var url = site + user;
            if (id != null)
            {
                url += Id(id);
            }

            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<User[]>(html);
        }

        public User[] GetUser(string name = null)
        {
            var url = site + user;
            if (name != null)
            {
                url += Name(name);
            }

            string html = GetUrl(url);
            return JsonConvert.DeserializeObject<User[]>(html);
        }
    }
}