using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class KurirRs : AbstractDownloader
    {
        public KurirRs(List<string> links)
        {
            this.links = links;
        }


        override public Page DownloadPage(Uri link)
        {
            HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
            KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
            List<string> authors = new List<string>();
            HtmlNode mainnode = doc.DocumentNode.SelectSingleNode("//article");
            if (mainnode == null)
                mainnode = doc.DocumentNode.SelectSingleNode("//div[@class='article ']");
            //HtmlNode pretexNode = mainnode.SelectSingleNode("//h4");
            HtmlNode pretextNode = mainnode.SelectSingleNode("//section[@class='detailViewIntro']");

            HtmlNodeCollection paragraphs = mainnode.SelectNodes("//section[@class='detailViewContent']/p");
            if (paragraphs == null)
                throw new NullReferenceException(link + " doesn't contain paragraph!!!");

            HtmlNode authorNode = mainnode.SelectSingleNode("//p[@class='contentAuthor']");

            if (authorNode != null)
            {
                try
                {
                    paragraphs.Remove(authorNode);
                    foreach (HtmlNode n in authorNode.SelectNodes("//span[@itemprop='name']"))
                    {
                        authors.Add(n.InnerText.Trim());
                    }
                }
                catch (ArgumentOutOfRangeException) { ;}
            }

            StringBuilder textBuilder = new StringBuilder();
            if (pretextNode != null)
                textBuilder.AppendLine(pretextNode.InnerText.Trim());
            foreach (HtmlNode p in paragraphs)
            {
                if (p.InnerText.Trim() != "")
                    textBuilder.AppendLine(p.InnerText.Trim());
            }

            Page page = new Page(link.AbsoluteUri, textBuilder.ToString(), baseInfo.Key);

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
            HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//div[@class='wrap']/time");
            Regex regexDate = new Regex(@"(\p{N}+)\.\s*(\p{N}+)\.\s*(\p{N}+)", RegexOptions.Compiled);
            Match m = regexDate.Match(pubNode.InnerText);
            int year = Convert.ToInt32(m.Groups[3].ToString());
            int monthInt = Convert.ToInt32(m.Groups[2].ToString());
            int day = Convert.ToInt32(m.Groups[1].ToString());
            return new DateTime(year, monthInt, day, 11, 11, 11);
        }

        private List<string> GetCategory(HtmlDocument doc)
        {
            HtmlNodeCollection catNode = doc.DocumentNode.SelectNodes("//div[@class='wrap']/span[@class='category']");
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
