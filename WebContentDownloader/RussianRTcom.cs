using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class RussianRTcom : AbstractDownloader
    {
        public RussianRTcom(List<string> links)
        {
            this.links = links;
        }


        override public Page DownloadPage(Uri link)
        {
            HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
            KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
            List<string> authors = new List<string>();
            HtmlNode mainnode = doc.DocumentNode.SelectSingleNode("//div[@class='article']");
            if(mainnode==null)
                mainnode = doc.DocumentNode.SelectSingleNode("//div[@class='article ']");
            //HtmlNode pretexNode = mainnode.SelectSingleNode("//h4");
            HtmlNode summaryNode = mainnode.SelectSingleNode("div[@class='article__summary']");

            HtmlNodeCollection paragraphs = mainnode.SelectNodes("div[@class='article__text']/p");
            if (paragraphs == null)
                throw new NullReferenceException(link + " doesn't contain paragraph!!!");

            HtmlNode authorNode = mainnode.SelectSingleNode("//div[@class='autors']");

            if (authorNode != null)
            {
                authors.AddRange(authorNode.InnerText.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }

            StringBuilder textBuilder = new StringBuilder();

            foreach (HtmlNode p in paragraphs)
            {
                if (p.InnerText.Trim() != "")
                    textBuilder.AppendLine(p.InnerText.Trim());
            }

            Page page = new Page(link.AbsoluteUri, textBuilder.ToString(), baseInfo.Key);
            if (summaryNode != null)
                page.Summary = summaryNode.InnerText.Trim();
            page.Keywords = baseInfo.Value;
            page.Categories = GetCategory(doc);
            page.Author = authors;
            page.PublishDate = GetPublishDate(doc);

            return page;

            throw new NotImplementedException();
        }

        private DateTime GetPublishDate(HtmlDocument doc)
        {
            //span[@class="titulek-pubtime"]   před 40 minutami / včera / datum </span>
            HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//div[@class='article']/div[@class='article__date']");
            Regex regexDate = new Regex(@"(\p{N}+)\s+(.+?)\s+(\p{N}+),", RegexOptions.Compiled);
            //02. máj 2016
            Match m = regexDate.Match(pubNode.InnerText);
            if (!m.Success)
                return new DateTime(0);
            int day = Convert.ToInt32(m.Groups[1].ToString());
            int year = Convert.ToInt32(m.Groups[3].ToString());

            int monthInt = 1;
            string month = m.Groups[2].ToString().ToLower().Trim();
            switch (month)
            {
                case "января": monthInt = 1; break;
                case "февраля": monthInt = 2; break;
                case "марта": monthInt = 3; break;
                case "апреля": monthInt = 4; break;
                case "мая": monthInt = 5; break;
                case "июня": monthInt = 6; break;
                case "июля": monthInt = 7; break;
                case "августа": monthInt = 8; break;
                case "сентября": monthInt = 9; break;
                case "октября": monthInt = 10; break;
                case "ноября": monthInt = 11; break;
                case "декабря": monthInt = 12; break;

                default:
                    monthInt = 1;
                    break;
            }
            return new DateTime(year, monthInt, day, 11, 11, 11);
        }

        private List<string> GetCategory(HtmlDocument doc)
        {
            HtmlNodeCollection catNode = doc.DocumentNode.SelectNodes("//div[@class='article']//a[@rel='tag']");
            List<string> cats = new List<string>();
            if (catNode != null)
            {
                foreach (HtmlNode n in catNode)
                {
                    string title = n.InnerText;
                    cats.Add(title.ToLower().Trim());
                }
            }
            return cats;
        }
    }
}
