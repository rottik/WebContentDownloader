using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class AktualneCZ : AbstractDownloader
    {
            /*
                    delete //div[@class='clanek-box-maly']
                    author //a[@class='autor']
                    //div[@class="clanek"]  sport, nazory !!!!
                    //span[@class="titulek-pubtime"]   před 40 minutami / včera / datum </span>
            */
            WebClient client;
            public AktualneCZ(List<string> links)
            {
                this.links = links;
                client = new WebClient();
            }

            override public Page DownloadPage(Uri link)
            {
                HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
                KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
                List<string> authors = new List<string>();
                HtmlNode pretexNode = doc.DocumentNode.SelectSingleNode("//div[@class='clanek']/div[@class='perex']");
                HtmlNodeCollection paragraphs = doc.DocumentNode.SelectNodes("//div[@class='clanek']/p");
                if (paragraphs == null)
                    throw new NullReferenceException(link + " doesn't contain paragraphs!!!");
                HtmlNode authorNode = doc.DocumentNode.SelectSingleNode("//div[@class='clanek']/p[@class='clanek-autor']");
                if (authorNode != null)
                {
                    paragraphs.Remove(authorNode);
                    string authorsString = authorNode.InnerText;
                    authorsString = authorsString.Substring(authorsString.IndexOf(':') + 1).Trim();
                    authors.AddRange(authorsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
                }

                StringBuilder textBuilder = new StringBuilder();
                try
                {
                    textBuilder.AppendLine(pretexNode.InnerText);
                }
                catch (NullReferenceException) { ;}

                foreach (HtmlNode node in paragraphs)
                    textBuilder.AppendLine(node.InnerText);

                Page page = new Page(link.AbsoluteUri, textBuilder.ToString(), baseInfo.Key);
                page.Keywords = baseInfo.Value;
                page.Categories = GetCategory(doc);
                page.Author = authors;
                page.PublishDate = GetPublishDate(doc);

                return page;
            }

            private DateTime GetPublishDate(HtmlDocument doc)
            {
                //span[@class="titulek-pubtime"]   před 40 minutami / včera / datum </span>
                HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//span[@class='titulek-pubtime']");
                if (pubNode == null)
                    return new DateTime(0);
                string dateString = pubNode.InnerText.Trim().ToLower();
                DateTime pubDate = DateTime.Now;
                Regex NumberRegex = new Regex(@"(\p{N}+)");
                if (dateString.Contains("před"))
                {
                    int number = Convert.ToInt32(NumberRegex.Match(dateString).Groups[1].ToString());
                    if (dateString.Contains("minut"))
                        pubDate = pubDate.AddMinutes(-1 * number);
                    if (dateString.Contains("hodin"))
                        pubDate = pubDate.AddHours(-1 * number);
                }
                else if (dateString.Contains("včera"))
                {
                    pubDate.AddDays(-1.0);
                }
                else
                {
                    Match m = Regex.Match(dateString, @"(\p{N}+)\.\s*(\p{N}+)\.\s*(\p{N}+)");
                    int day = Convert.ToInt32(m.Groups[1].ToString());
                    int month = Convert.ToInt32(m.Groups[2].ToString());
                    int year = Convert.ToInt32(m.Groups[3].ToString());
                    return new DateTime(year, month, day, 11, 11, 11);
                }
                return pubDate;
            }

            private List<string> GetCategory(HtmlDocument doc)
            {
                HtmlNode catNode = doc.DocumentNode.SelectSingleNode("//div[@class='menu']/ul/li/a[@class='active']");
                List<string> cats = new List<string>();
                if (catNode != null)
                    cats.Add(catNode.InnerText.ToLower().Trim());
                return cats;
            }
    }
}
