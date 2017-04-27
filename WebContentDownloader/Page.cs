using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebContentDownloader
{
    class Page
    {
        public Page(string link, string text, string title)
        {
            this.link = link;
            this.text = text;
            this.title = title;
        }

        public string ToXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<article>");
            sb.AppendLine("\t<link>" + this.link + "</link>");
            sb.AppendLine("\t<title>" + this.title + "</title>");
            sb.AppendLine("\t<authors>");
            foreach (string aut in this.Author)
                sb.AppendLine("\t\t<author>" + aut + "</author>");
            sb.AppendLine("\t</authors>");
            sb.AppendLine("\t<categories>");
            foreach (string aut in this.Categories)
                sb.AppendLine("\t\t<category>" + aut + "</category>");
            sb.AppendLine("\t</categories>");
            sb.AppendLine("\t<keywords>");
            foreach (string aut in this.keywords)
                sb.AppendLine("\t\t<keyword>" + aut + "</keyword>");
            sb.AppendLine("\t</keywords>");
            sb.AppendLine("\t<date>" + this.PublishDate + "</date>");
            sb.AppendLine("\t<summary>" + this.Summary + "</summary>");
            sb.AppendLine("\t<text>" + this.Text + "</text>");
            sb.AppendLine("</article>");
            return sb.ToString();
        }

        string link;

        public string Link
        {
            get { return link; }
            set { link = value; }
        }
        string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        List<string> categories;

        public List<string> Categories
        {
            get { return categories; }
            set { categories = value; }
        }
        List<string> keywords;

        public List<string> Keywords
        {
            get { return keywords; }
            set { keywords = value; }
        }
        DateTime publishDate;

        public DateTime PublishDate
        {
            get { return publishDate; }
            set { publishDate = value; }
        }
        string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        string summary;

        public string Summary
        {
            get { return summary; }
            set { summary = value; }
        }
        List<string> author;

        public List<string> Author
        {
            get { return author; }
            set { author = value; }
        }
    }
}
