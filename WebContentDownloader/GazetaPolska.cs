using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class GazetaPolska : AbstractDownloader
    { /*
             * http://www.gazetapolska.pl/archiwum/artykuly
             */

        public GazetaPolska(List<string> links)
        {
            this.links = links;
        }

        override public Page DownloadPage(Uri link)
        {

            HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
            KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
            List<string> authors = new List<string>();

            HtmlNode pretexNode = doc.DocumentNode.SelectSingleNode("//div[@id='content-page-left']//div[@class='content']/strong");

            List<string> xpaths2remove = new List<string>();
            xpaths2remove.Add("//div[@style='color: #868686; font-size: 0.8em']");
            xpaths2remove.Add("//div[@class='kiosk']");
            xpaths2remove.Add("//div[@class='meta']");
            xpaths2remove.Add("//div[@class='meta-social']");
            xpaths2remove.Add("//div[@class='links']");
            xpaths2remove.Add("//a[@class='drukuj']");

            HtmlNode paragraph = doc.DocumentNode.SelectSingleNode("//div[@id='content-page-left']//div[@class='content']");
            if (paragraph == null)
                throw new NullReferenceException(link + " doesn't contain paragraph!!!");
            foreach (string xpath in xpaths2remove)
                try
                {
                    paragraph.RemoveChild(paragraph.SelectSingleNode(xpath), false);
                }
                catch (Exception) { ;}

            HtmlNode authorNode = doc.DocumentNode.SelectSingleNode("//div[@id='content-page-right']//div[@class='content']//span/h1");

            if (authorNode != null)
            {
                authors.Add(authorNode.InnerText);
            }

            StringBuilder textBuilder = new StringBuilder();
            foreach (string line in paragraph.InnerText.Replace("<!-- /social -->", "").Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Trim() != "")
                    textBuilder.AppendLine(line.Trim());
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
            HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("/html/head/meta[@name='keywords']");
            string content = pubNode.Attributes["content"].Value;
            string[] parts = content.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            string dateStrting = "";
            Regex regexDate = new Regex(@"z\s+(\p{N}+)\s+(.+?)\s+(\p{N}+)", RegexOptions.Compiled);
            foreach (string part in parts)
            {
                if (regexDate.IsMatch(part))
                {
                    dateStrting = part;
                    break;
                }
            }

            Match m = regexDate.Match(dateStrting);
            if (!m.Success)
                return new DateTime(0);
            int day = Convert.ToInt32(m.Groups[1].ToString());
            int year = Convert.ToInt32(m.Groups[3].ToString());

            int monthInt = 1;
            string month = m.Groups[2].ToString().ToLower().Trim();
            switch (month)
            {
                case "stycznia": monthInt = 1; break;
                case "lutego": monthInt = 2; break;
                case "marca": monthInt = 3; break;
                case "kwietnia": monthInt = 4; break;
                case "maja": monthInt = 5; break;
                case "czerwca": monthInt = 6; break;
                case "lipca": monthInt = 7; break;
                case "sierpnia": monthInt = 8; break;
                case "września": monthInt = 9; break;
                case "października": monthInt = 10; break;
                case "listopada": monthInt = 11; break;
                case "grudnia": monthInt = 12; break;

                default:
                    monthInt = 1;
                    break;
            }
            return new DateTime(year, monthInt, day, 11, 11, 11);

        }

        private List<string> GetCategory(HtmlDocument doc)
        {
            HtmlNode catNode = doc.DocumentNode.SelectSingleNode("//div[@id='content-page-left']//div[@class='content']/span");
            List<string> cats = new List<string>();
            if (catNode != null)
            {
                string title = catNode.InnerText;
                int index =title.IndexOf("\\");
                if(index>0)
                    title = title.Substring(0, index);
                cats.Add(title.ToLower().Trim());
            }
            return cats;

        }
    }
}
