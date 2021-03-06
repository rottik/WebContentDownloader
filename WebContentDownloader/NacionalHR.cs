﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
    class NacionalHR:AbstractDownloader
    {
        public NacionalHR(List<string> links)
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

            HtmlNodeCollection paragraphs = mainnode.SelectNodes("//div[@class='td-post-content']/p");
            if (paragraphs == null)
                throw new NullReferenceException(link + " doesn't contain paragraph!!!");

            HtmlNode authorNode = mainnode.SelectSingleNode("//div[@class='td-post-author-name']/a[@itemprop='author']");

            if (authorNode != null)
            {
                authors.AddRange(authorNode.InnerText.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
            }

            StringBuilder textBuilder = new StringBuilder();
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
        }

        private DateTime GetPublishDate(HtmlDocument doc)
        {
            //span[@class="titulek-pubtime"]   před 40 minutami / včera / datum </span>
            HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//time[@itemprop='dateCreated']");
            string content = pubNode.InnerText;
            Regex regexDate = new Regex(@"(\p{N}+)\.\s*(\p{N}+)\.\s*(\p{N}+)", RegexOptions.Compiled);
            Match m = regexDate.Match(content);
            if (!m.Success)
                return new DateTime(0);
            int day = Convert.ToInt32(m.Groups[1].ToString());
            int month = Convert.ToInt32(m.Groups[2].ToString());
            int year = Convert.ToInt32(m.Groups[3].ToString());

            return new DateTime(year, month, day, 11, 11, 11);

        }

        private List<string> GetCategory(HtmlDocument doc)
        {
            HtmlNodeCollection catNode = doc.DocumentNode.SelectNodes("//ul[@class='td-category']/li[@class='entry-category']");
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
