using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebContentDownloader
{
        class AktualneSK : AbstractDownloader
        {
            public AktualneSK(List<string> links)
            {
                this.links = links;
            }

            override public Page DownloadPage(Uri link)
            {

                HtmlDocument doc = this.GetHtmlDocumentFromLink(link);
                KeyValuePair<string, List<string>> baseInfo = GetTitleAndKeywords(doc);
                List<string> authors = new List<string>();
                HtmlNode mainnode = doc.DocumentNode.SelectSingleNode("//section/div[contains(@class,'article')]");
                HtmlNode pretexNode = mainnode.SelectSingleNode("//h4");

                HtmlNodeCollection paragraphs = mainnode.SelectNodes("p");
                if (paragraphs == null)
                    throw new NullReferenceException(link + " doesn't contain paragraph!!!");

                HtmlNode authorNode = mainnode.SelectSingleNode("//div[@class='autors']");

                if (authorNode != null)
                {
                    authors.AddRange(authorNode.InnerText.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }

                StringBuilder textBuilder = new StringBuilder();
                if (pretexNode != null)
                    textBuilder.AppendLine(pretexNode.InnerText);

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
                HtmlNode pubNode = doc.DocumentNode.SelectSingleNode("//span[@class='date']");
                HtmlNode wordNode = pubNode.SelectSingleNode("//span[@class='word']");
                if (wordNode != null)
                {
                    if (wordNode.InnerText.ToLower().Contains("dnes"))
                        return DateTime.Now;
                    else if (wordNode.InnerText.ToLower().Contains("včera"))
                        return DateTime.Now.AddDays(-1.0);
                    else
                    {
                        Regex regexDate = new Regex(@"(\p{N}+)\.\s+(.+?)\s+(\p{N}+)", RegexOptions.Compiled);
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
                            case "január": monthInt = 1; break;
                            case "február": monthInt = 2; break;
                            case "marec": monthInt = 3; break;
                            case "apríl": monthInt = 4; break;
                            case "máj": monthInt = 5; break;
                            case "jún": monthInt = 6; break;
                            case "júl": monthInt = 7; break;
                            case "august": monthInt = 8; break;
                            case "september": monthInt = 9; break;
                            case "október": monthInt = 10; break;
                            case "november": monthInt = 11; break;
                            case "december": monthInt = 12; break;

                            default:
                                monthInt = 1;
                                break;
                        }
                        return new DateTime(year, monthInt, day, 11, 11, 11);
                    }
                }
                return new DateTime(0);
                //<span class="word">dnes</span>
                //<span class="word">včera</span>
                //02. máj 2016, 18:20      
            }

            private List<string> GetCategory(HtmlDocument doc)
            {
                HtmlNodeCollection catNode = doc.DocumentNode.SelectNodes("//ul[@class='article__head']/li[@class='link']");
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

