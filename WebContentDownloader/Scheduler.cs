using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebContentDownloader
{
    enum Mode { Getlinks, GetContent };

    class Scheduler
    {
        Random rnd = new Random();
        bool isRunnig = false;
        List<Task> tasks = new List<Task>();
        Dictionary<string, Uri> servers;
        uint limit;
        string datadir = @"D:\classification\xml\";

        public bool IsRunnig
        {
            get { return isRunnig; }
        }

        public Scheduler(Dictionary<string, Uri> targetServers)
        {
            this.servers = targetServers;
            limit = UInt32.MaxValue;
        }

        public Scheduler(Dictionary<string, Uri> targetServers, uint linkLimit)
        {
            this.servers = targetServers;
            this.limit = linkLimit;
        }

        public void GetLinks()
        {
            foreach (var server in servers)
            {
                Task t = new Task(() =>
                {
                    AbstractDownloader ad;
                    ad = new AbstractDownloader(Mode.Getlinks, server.Value, 10, limit, server.Key);
                    List<string> processedLinks = new List<string>();
                    List<string> links = new List<string>();
                    if (File.Exists(server.Key + "-processed-links.txt"))
                        processedLinks.AddRange(File.ReadAllLines(server.Key + "-processed-links.txt"));
                    if (File.Exists(server.Key + "-links.txt"))
                        links.AddRange(File.ReadAllLines(server.Key + "-links.txt"));
                    ad.Continue(links, processedLinks);
                }
                );
                tasks.Add(t);
                t.Start();
            }
            isRunnig = true;
            Task.WaitAll(tasks.ToArray());
            isRunnig = false;
        }

        public void GetContent()
        {
            foreach (var server in servers)
            {
                Task t = new Task(() =>
                    {
                        List<string> links = File.ReadAllLines(server.Key + "-links.txt").ToList();
                        AbstractDownloader ad;
                        switch (server.Key)
                        {
                            case "nacional.hr":
                                ad = new NacionalHR(links);
                                break;
                            case "delo.si":
                                ad = new deloSI(links);
                                break;
                            case "kurir.rs":
                                ad = new KurirRs(links);
                                break;
                            case "russian.rt.com":
                                ad = new RussianRTcom(links);
                                break;
                            case "aktualne.atlas.sk":
                                ad = new AktualneSK(links);
                                break;
                            case "gazetapolska.pl":
                                ad = new GazetaPolska(links);
                                break;
                            case "aktualne.cz":
                                ad = new AktualneCZ(links);
                                break;
                            default:
                                throw new NotSupportedException("Server " + server.Key + " is not supperted!!!");
                        }
                        List<Page> pages = new List<Page>();
                        uint fileCounter = 1;
                        HashSet<string> parsedLinks = new HashSet<string>();
                        string filename;
                        foreach (string uri in links.OrderBy(sd => rnd.Next()))
                        {
                            try
                            {
                                pages.Add(ad.DownloadPage(new Uri(uri)));
                                parsedLinks.Add(uri);
                            }
                            catch (NullReferenceException nre) { File.AppendAllLines(server.Key + "_error.txt", new string[] { nre.Message + "\t" + uri }); }

                            if (pages.Count == 100)
                            {
                                do
                                {
                                    filename = datadir + server.Key + "_" + fileCounter + ".xml";
                                    fileCounter++;
                                } while (File.Exists(filename));
                                SavePages(pages, filename);
                                Console.WriteLine((fileCounter * 100) + " were saved.");
                                File.AppendAllLines(datadir + server.Key + "-parsedLinks.txt", parsedLinks);
                                pages.Clear();
                            }
                        }
                        do
                        {
                            filename = datadir + server.Key + "_" + fileCounter + ".xml";
                            fileCounter++;
                        } while (File.Exists(filename));
                        SavePages(pages, filename);
                        File.WriteAllLines(server.Key + "-parsedLinks.txt", parsedLinks);
                    });
                tasks.Add(t);
                t.Start();
            }
            isRunnig = true;
            Task.WaitAll(tasks.ToArray());
            isRunnig = false;
        }

        public void SavePages(List<Page> pages, string XmlName)
        {
            TextWriter tw = new StreamWriter(XmlName);
            tw.WriteLine("<pages>");
            foreach (Page page in pages)
            {
                tw.WriteLine("\n" + page.ToXML() + "\n");
            }
            tw.WriteLine("</pages>");
            tw.Close();
        }

        public List<Page> GetPages(List<string> links)
        {
            throw new NotImplementedException();
        }
    }
}
