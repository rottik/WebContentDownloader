using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebContentDownloader
{


    interface DownloaderInterface
    {
        //  List<string> GetLinks(int limit);
        //   List<string> GetLinks(DateTime day);
        Page DownloadPage(Uri link);
        List<Page> DownloadPageSet(List<Uri> links, string server, uint stepToSave);
    }

    class AbstractDownloader : DownloaderInterface
    {
        Mode mode;
        List<string> processedLinks = new List<string>();
        protected List<string> links = new List<string>();
        private uint stepLimit, NumberOfLinks;
        private KeyValuePair<string, Uri> server;

        public List<string> GetLinks(int limit)
        {
            throw new NotImplementedException("Not implemented yet");
        }
        public List<string> GetLinks(DateTime day)
        {
            throw new NotImplementedException("Not implemented yet");
        }
        virtual public Page DownloadPage(Uri link)
        {
            throw new NotImplementedException("Not implemented yet");
        }
        public List<Page> DownloadPageSet(List<Uri> links, string server, uint stepToSave)
        {
            List<Page> pages = new List<Page>();
            foreach (Uri uri in links)
            {
                pages.Add(DownloadPage(uri));
                if (pages.Count % stepToSave == 0)
                    SavePages(server + "-pages.xml");
            }
            return pages;
        }
        public AbstractDownloader()
        { ;}
        public AbstractDownloader(Mode mode, Uri page, uint steplimit, uint NumberOfLinks, string serverLink)
        {
            this.mode = mode;
            this.NumberOfLinks = NumberOfLinks;
            this.stepLimit = steplimit;
            server = new KeyValuePair<string, Uri>(serverLink, page);
            links = new List<string>();
            links.Add(page.AbsoluteUri);
        }
        public void SavePages(string file)
        {

        }
        public void Run()
        {
            if (this.mode == Mode.Getlinks)
            {
                Run(NumberOfLinks);
            }
        }
        public void Run(uint limit)
        {
            if (this.mode == Mode.Getlinks)
            {
                while (processedLinks.Count <= limit)
                {
                    if (links.Count == 0)
                        break;
                    string link = links.First();
                    List<string> foundedLinks = GetLinksFromPage(new Uri(link), links, server.Key);
                    Thread.Sleep(100);
                    links.AddRange(foundedLinks);
                    processedLinks.Add(link);
                    links = links.Except(processedLinks).Distinct().OrderBy(p=>p.Length).ToList();
                    if (links.Count % stepLimit == 0)
                    {
                        Console.WriteLine(server.Key + "\t" + links.Count + "\tsaved.");
                        this.SaveProcessedLinks(server.Key + "-processed-links.txt");
                        this.SaveAllLinks(server.Key + "-links.txt");
                    }
                }
            }
        }

        public void Continue(List<string> links, List<string> crawledLinks)
        {
            this.links = links;
            this.processedLinks = crawledLinks;
            Run();
        }

        public List<string> ProcessedLinks
        {
            get { return processedLinks; }
            set { processedLinks = value; }
        }
        public void SaveProcessedLinks(string file)
        {
            File.WriteAllLines(file, processedLinks);
        }
        public void SaveAllLinks(string file)
        {
            File.WriteAllLines(file, links);
        }
        public List<string> GetLinksFromPage(Uri page, List<string> prevListOfPages, string server)
        {

            HtmlDocument doc = new HtmlDocument();
            WebClient client = new WebClient();
            try
            {
                doc.LoadHtml(client.DownloadString(page.AbsoluteUri));
            }
            catch (WebException we)
            {
                Console.WriteLine(page.AbsoluteUri + "\tcaused webexception " + we.Message);
                return prevListOfPages;
            }
            HtmlNodeCollection nc = doc.DocumentNode.SelectNodes("//a");
            if (nc == null)
                return prevListOfPages;
            foreach (HtmlNode aNode in nc)
            {
                string origHref;
                try
                {
                    origHref = aNode.Attributes["href"].Value;
                }
                catch (NullReferenceException) { continue; }
                if (origHref.Length <= server.Length) continue;
                if (origHref.StartsWith("#") || origHref.Contains("mailto:"))
                    continue;
                string href = origHref;
                if (href.StartsWith("//"))
                    href = "http:" + href;
                bool checkedLink = false;
                while (!checkedLink)
                {
                    try
                    {
                        Uri link = new Uri(href);
                        href = link.AbsoluteUri;
                        checkedLink = true;
                    }
                    catch (UriFormatException ufe)
                    {
                        href = Try2Correctlink(href, page);
                    }
                }
                if (!prevListOfPages.Contains(href))
                    if (href.Contains(server) && IsValid(href))
                        prevListOfPages.Add(href);
            }
            //Console.WriteLine(page.AbsoluteUri + "\t processed!");
            return prevListOfPages;
        }
        private bool IsValid(string href)
        {
            bool valid = true;
            List<string> exceptions = new List<string>();
            exceptions.Add("twitter.com");
            exceptions.Add("pinterest.com");
            exceptions.Add("facebook.com");

            foreach (string exception in exceptions)
                if (href.Contains(exception))
                    return false;
            return true;
        }
        static string Try2Correctlink(string link, Uri page)
        {
            if (link.StartsWith("/") || link.StartsWith("."))
                return page.Host + link;
            if (!link.StartsWith("h"))
                return "http://" + link;
            return page + link;
        }
        static public HtmlDocument ReencodePageString(HtmlDocument doc, Encoding e)
        {
            Encoding targetEnc=null;
            if (doc.DeclaredEncoding == null)
            {
                HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/head/meta[@charset]");
                if (node == null)
                {
                    targetEnc = Encoding.UTF8;
                }
                else
                {
                    string encString = node.Attributes["charset"].Value.ToLower();
                    if(encString=="utf-8")
                        targetEnc = Encoding.UTF8;
                    ;//<meta charset="UTF-8">
                }
            }
            else
            {
                targetEnc = doc.DeclaredEncoding;
            }


            if (e != targetEnc)
            {
                string text = targetEnc.GetString(e.GetBytes(doc.DocumentNode.OuterHtml));
                doc = new HtmlDocument();
                doc.LoadHtml(HttpUtility.HtmlDecode(text));
            }
            return doc;
        }
        protected HtmlDocument GetHtmlDocumentFromLink(Uri link)
        {
            WebClient client = new WebClient();
            byte[] buffer = new byte[0];
            try
            {
                buffer = client.DownloadData(link);
            }
            catch (WebException we)
            {
                Console.WriteLine(link + " caused " + we.Message);
            }
            if (buffer.Length == 0)
            {
                return new HtmlDocument();
            }

            string pageString = Encoding.UTF8.GetString(buffer);
            pageString = pageString.Replace("&nbsp;", " ").Replace("&shy;", "").Replace(" \">","\">");
            pageString = System.Web.HttpUtility.HtmlDecode(pageString);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageString);
            doc = ReencodePageString(doc, Encoding.UTF8);
            return doc;
        }
        protected KeyValuePair<string, List<string>> GetTitleAndKeywords(HtmlDocument doc)
        {
            string title;
            List<string> keywords = new List<string>();
            //title /html/head/title
            //keywords /html/head/meta[@name='keywords']  split(new string[] {","},StringSplitOption.RemoveEmptyEntries).Select(p=>p.Trim())
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("/html/head/title");

            if (titleNode == null)
                title = "";
            else
                title = titleNode.InnerText;

            HtmlNode keywordsNode = doc.DocumentNode.SelectSingleNode("/html/head/meta[@name='keywords']");
            if (keywordsNode != null)
            {
                string keyws = keywordsNode.Attributes["content"].Value;
                keywords.AddRange(keyws.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
            }

            return new KeyValuePair<string, List<string>>(title, keywords);
        }
    }

}
