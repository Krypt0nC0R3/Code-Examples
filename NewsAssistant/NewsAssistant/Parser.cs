using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace NewsAssistant
{
    public class NewsParser
    {
        public class News
        {
            public string Title;
            public string Text;
            public Bitmap Picture;
            public string News_URL;
            public DateTime Date;

            public News() { }
        }

        public string Main_URL;
        public List<string> URLs = null;

        public List<News> news = null;

        public bool IsParsing{get;private set;}


        public event PageParsed OnPageParsed;
        public delegate void PageParsed(ParserEventArgs args);

        public void Optimize()
        {
            if (news != null)
            {
                int id = GC.GetGeneration(news);
                news.Clear();
                news.TrimExcess();
                GC.Collect(id, GCCollectionMode.Forced);
                news = null;
            }
        }

        public NewsParser(string URL)
        {
            Main_URL = URL;
        }

        public NewsParser(List<string> URLs)
        {
            this.URLs = URLs;
        }

        public class ParserEventArgs
        {
           public int Max, Value, NewsCount, PagesCount;
        }
        public bool SearchNews()
        {
            IsParsing = true;
            news = new List<News>();
            if (URLs == null)
            {
                IsParsing = false;
                return ParseNews(Main_URL);
            }
            else
            {
                bool parsed = false;
                for(int i = 0; i < URLs.Count; i++)
                {
                    bool p = ParseNews(URLs[i]);
                    ParserEventArgs arg = new ParserEventArgs()
                    {
                        Max = URLs.Count - 1,
                        Value = i,
                        NewsCount = news.Count,
                        PagesCount = URLs.Count
                    };
                    OnPageParsed?.Invoke(arg);
                    parsed = parsed || p;
                }
                IsParsing = false;
                return parsed;
            }

        }

        private bool ParseNews(string URL)
        {
            HtmlParser parser = new HtmlParser();
            string html = null;
            try
            {
                html = GetHTML(URL);
            }
            catch
            {
                Thread.Sleep(2000);
                try
                {
                    html = GetHTML(URL);
                }
                catch
                {

                }
            }
            if (html == null)
            {
                return false;
            }
            IHtmlDocument document = parser.ParseDocument(html);
            List<IElement> items = document.All.Where(m => m.LocalName == "div" && m.ClassList.Contains("item") && m.ClassList.Contains("type-post") && m.ClassList.Contains("status-publish")).ToList();

            

            foreach (IElement element in items)
            {
                string title = "";
                string text = "";
                string url = "";
                Bitmap picture;
                DateTime date;

                title = element.QuerySelector("div.item-content").Children.First(m => m.LocalName == "h3").Children.First(m => m.LocalName == "a").Text();
                url = element.QuerySelector("div.item-content").Children.First(m => m.LocalName == "h3").Children.First(m => m.LocalName == "a").GetAttribute("href");
                string d = element.QuerySelector("div.item-content").Children.First(m => m.LocalName == "h3").Children.First(m => m.LocalName == "p").Text();
                string year = url.Substring(url.IndexOf('/') + 13);
                year = year.Substring(0, year.IndexOf('/'));
                string[] splt = d.Split(' ');
                d = splt[0] + " " + splt[1] + " " + year + " " + splt[2];
                date = DateTime.Parse(d);
                string htmll = null;
                try
                {
                    htmll = GetHTML(url);
                }
                catch
                {
                    Thread.Sleep(2000);
                    try
                    {
                        htmll = GetHTML(url);
                    }
                    catch
                    {

                    }
                }
                if (htmll == null)
                {
                    return false;
                }

                IHtmlDocument doc = parser.ParseDocument(htmll);

                text = doc.QuerySelectorAll(".caps").First().Text();

                IElement img = null;
                foreach (IElement elem in doc.All)
                {
                    bool isWP = false;
                    foreach (var clas in elem.ClassList)
                    {
                        if (clas.StartsWith("wp-image-"))
                        {
                            isWP = true;
                            break;
                        }
                    }
                    if (isWP && elem.LocalName == "img")
                    {
                        img = elem;
                        break;
                    }
                }

                using (WebClient wc = new WebClient())
                {
                    using (Stream s = wc.OpenRead(img?.GetAttribute("src")))
                    {
                        picture = new Bitmap(s);
                    }
                }

                News n = new News()
                {
                    Title = title,
                    Text = text,
                    Picture = picture,
                    News_URL = url,
                    Date = date
                };

                int ind = news.FindIndex(x => x.Title == title);

                if (!news.Contains(n) && ind == -1)
                {
                    news.Add(n);
                }
            }
            return true;
        }

        private string GetHTML(string urlAddress)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string data = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();


            }
            return data;
            
        }


    }
}
