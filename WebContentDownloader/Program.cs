using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebContentDownloader
{
    class Program
    {
        static void Main(string[] args)
        {

            Dictionary<string, Uri> servers = new Dictionary<string, Uri>();
            //servers.Add("idnes.cz", new Uri("http://www.idnes.cz"));
            //servers.Add("novinky.cz", new Uri("http://www.novinky.cz"));
            servers.Add("nacional.hr", new Uri("http://www.nacional.hr/"));
            servers.Add("delo.si", new Uri("http://www.delo.si/"));
            servers.Add("kurir.rs", new Uri("http://www.kurir.rs/"));
            servers.Add("russian.rt.com", new Uri("https://russian.rt.com/"));
            servers.Add("aktualne.atlas.sk", new Uri("http://aktualne.atlas.sk/"));
            servers.Add("gazetapolska.pl", new Uri("http://www.gazetapolska.pl/archiwum/artykuly"));
            servers.Add("aktualne.cz", new Uri("http://www.aktualne.cz"));

            Scheduler s = new Scheduler(servers,1500000);
            //s.GetLinks();
            //while (s.IsRunnig)
            //{
            //    Thread.Sleep(1000);
            //}
            s = new Scheduler(servers);
            s.GetContent();
            Console.WriteLine();
        }
    }
}
