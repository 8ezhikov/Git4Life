using System.Net;
using System.Net.Sockets;

namespace CrawlerClient
{
    internal class ClientHelper
    {
        public static string GetLocalIP()
        {
            var localIp = "?";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                }
            }
            return localIp;
        }

        public static string GetPublicIP()
        {
            var webClient = new WebClient();

            var ip = webClient.DownloadString("http://icanhazip.com/");

            return ip;
        }
    }
}