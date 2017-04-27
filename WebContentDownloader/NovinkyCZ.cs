using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebContentDownloader
{
    class NovinkyCZ : AbstractDownloader
    {
        /*
         * <div id="subpage"><div id="main"><div id="navigation"><ul><li data-dot="c_hlavni_stranka"><li data-dot="c_rubrika"><a href="http://www.novinky.cz/krimi/">Krimi</a>
                //p[@class="publicDate"]
                //p[@class="perex"]+//div[@class="articleBody"]
                //p[@class="articleAuthors"] 
        */

        public NovinkyCZ(List<string> links)
        {
            this.links = links;
        }


        override public Page DownloadPage(Uri link)
        {
            /*
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
            */
            throw new NotImplementedException();
        }
    }
}
