using System.Net;
using System.Net.Sockets;

namespace CrawlerClient
{
    internal class ClientHelper
    {
        public static string GetLocalIP()
        {
            var localIP = "?";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
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
            var webClient = new WebClient();

            var ip = webClient.DownloadString("http://icanhazip.com/");

            return ip;
        }
    }
}