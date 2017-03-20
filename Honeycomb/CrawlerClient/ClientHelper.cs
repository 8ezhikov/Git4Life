using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using CsvHelper;
using Microsoft.Win32;

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

        public static void CreateCSV(List<Benchmark> benchList)
        {

            var save = new SaveFileDialog();
            save.Filter = "CSV Files (*.csv)|*.csv";
            save.DefaultExt = "csv";
            save.AddExtension = true;
            save.ShowDialog();
            using (TextWriter fileReader = File.CreateText(save.FileName))
            {
                var csv = new CsvWriter(fileReader);
                csv.WriteField("Benchmark Number");
                csv.WriteField("URL");
                csv.WriteField("Elapsed Time");
                csv.WriteField("Internal Links Count");
                csv.WriteField("External Links Count");
                csv.NextRecord();

                var totalTime = new TimeSpan();

                var totalIntLinksCount = 0;
                var totalExtLinksCount = 0;
                foreach (var item in benchList)
                {
                    csv.WriteField(item.BenchNumber);
                    csv.WriteField(item.WebSiteURL);
                    csv.WriteField(item.crawlingTime);
                    csv.WriteField(item.InternalLinksCount);
                    csv.WriteField(item.ExternalLinksCount);
                    csv.NextRecord();

                    totalTime += item.crawlingTime;
                    totalIntLinksCount += item.InternalLinksCount;
                    totalExtLinksCount += item.ExternalLinksCount;
                }

                csv.NextRecord();
                csv.WriteField("Total running Time:");
                csv.WriteField( totalTime);
                csv.NextRecord();

                csv.WriteField("Internal Links Count:");
                csv.WriteField( totalIntLinksCount);
                csv.NextRecord();

                csv.WriteField("External Links Count:");
                csv.WriteField(totalExtLinksCount);
                csv.NextRecord();
            }
        }

        public struct Benchmark
        {
            public int BenchNumber;
            public string WebSiteURL;
            public TimeSpan crawlingTime;
            public int InternalLinksCount;
            public int ExternalLinksCount;
        }
    }
}