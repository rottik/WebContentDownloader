using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class deloSI :AbstractDownloader
    {
        public deloSI(List<string> links)
        {
            this.links = links;
        }


        override public Page DownloadPage(Uri link)
        {
            HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
            KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
            List<string> authors = new List<string>();
            HtmlNode mainnode = doc.DocumentNode.SelectSingleNode("//div[@id='D_NEWS']");
            if (mainnode == null)
                mainnode = doc.DocumentNode.SelectSingleNode("//div[@class='article ']");
            //HtmlNode pretexNode = mainnode.SelectSingleNode("//h4");
            HtmlNode pretextNode = mainnode.SelectSingleNode("//div[@id='EXCERPT']");

            HtmlNodeCollection paragraphs = mainnode.SelectNodes("//div[@id='d_conmment_holder']/p");
            if (paragraphs == null)
                throw new NullReferenceException(link + " doesn't contain paragraph!!!");

            HtmlNode authorNode = mainnode.SelectSingleNode("//div[@class='d_author']");

            if (authorNode != null)
            {
                authors.AddRange(authorNode.InnerText.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
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
            HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//div[@class='date_clanek']");
            Regex regexDate = new Regex(@"(\p{N}+)\.\s*(\p{N}+)\.\s*(\p{N}+)", RegexOptions.Compiled);
            Match m = regexDate.Match(pubNode.InnerText);
            int year = Convert.ToInt32(m.Groups[3].ToString());
            int monthInt = Convert.ToInt32(m.Groups[2].ToString());
            int day = Convert.ToInt32(m.Groups[1].ToString());
            return new DateTime(year, monthInt, day, 11, 11, 11);
        }

        private List<string> GetCategory(HtmlDocument doc)
        {
            HtmlNodeCollection catNode = doc.DocumentNode.SelectNodes("//div[@id='MENU']//div[@id='BreadCrumbs']/a");
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
