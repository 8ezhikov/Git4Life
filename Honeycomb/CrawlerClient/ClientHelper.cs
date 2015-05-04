using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace CrawlerClient
{
    internal class ClientHelper
    {
        public static string GetLocalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public static string GetPublicIP()
        {
            Stopwatch start = Stopwatch.StartNew();


            var webClient = new WebClient();

            string IP = webClient.DownloadString("http://icanhazip.com/");
            start.Stop();

            return IP;
            //String direction = "";
            //WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            //using (WebResponse response = request.GetResponse())
            //using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            //{
            //    direction = stream.ReadToEnd();
            //}

            ////Search for the ip in the html
            //int first = direction.IndexOf("Address: ") + 9;
            //int last = direction.LastIndexOf("</body>");
            //start.Stop();
            //direction = direction.Substring(first, last - first);

            //return direction;
        }
    }
}