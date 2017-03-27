using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using CrawlerClient.CrawlerServer;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace CrawlerClient.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand StartTestCrawlingCommand { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }

        public bool IsConnectButtonActive
        {
            get { return _isConnectButtonActive; }
            set { _isConnectButtonActive = value; RaisePropertyChanged("IsConnectButtonActive"); }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ConnectToServerCommand = new RelayCommand(ConnectToServer);
            StartTestCrawlingCommand = new RelayCommand(TestCrawl);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            ServerAddress = "193.124.113.235:22222";
            ClientName = $"Denis Test Crawler {DateTime.Now}";
            CrawlerStatus = "Waiting";
            ConnectionSingleton.Instance.InjectViewModel(this);
            IsConnectButtonActive = true;
        }
        private void ConnectToServer()
        {
            IsConnectButtonActive = false;
            var singleTone = ConnectionSingleton.Instance;

            var newPerson = new ClientCrawlerInfo
            {
                ClientName = ClientName,
                ServerIP = LocalIpAddress
            };

            var result = singleTone.Connect(newPerson, ServerAddress);
            if (result)
                CrawlerStatus = "Connected";

        }
        private void CloseWindow()
        {
            ConnectionSingleton.Instance.Disconnect();
        }
        private void TestCrawl()
        {
            IsConnectButtonActive = false;

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var seed = new SeedDTO { SeedDomainName = "http://mathem.krc.karelia.ru/" };
            var seed1 = new SeedDTO { SeedDomainName = "http://mathem.krc.karelia.ru/" };
            var seed2 = new SeedDTO { SeedDomainName = "http://mathem.krc.karelia.ru/" };

            var seedList = new List<SeedDTO>();
            seedList.Add(seed);
            seedList.Add(seed1);
            seedList.Add(seed2);

            var benchList = new List<ClientHelper.Benchmark>();
            var crawlerInstance = new CrawlerEngine();

            var result = crawlerInstance.StartCrawlingProcess(seedList);
            var i = 0;
            foreach (var batchResult in result.BatchInfo.resultCollection)
            {
                var bench = new ClientHelper.Benchmark();
                bench.BenchNumber = i + 1;
                bench.WebSiteURL = batchResult.ProcessedSeed.SeedDomainName;

                bench.crawlingTime = batchResult.SiteCrawlingTime;

                bench.InternalLinksCount = batchResult.InternalLinksList.Count;
                bench.ExternalLinksCount = batchResult.ExternalLinksList.Count;
                benchList.Add(bench);
            }

            ClientHelper.CreateCSV(benchList);
            stopWatch.Stop();
            
        }

        private string _publicIpAdress;
        public string PublicIpAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_publicIpAdress))
                {
                    _publicIpAdress = ClientHelper.GetPublicIP();
                }

                return _publicIpAdress;
            }
        }

        private string _localIpAdress;
        private string _crawlerStatus;
        private bool _isConnectButtonActive;

        public string LocalIpAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_localIpAdress))
                {
                    _localIpAdress = ClientHelper.GetLocalIP();
                }

                return _localIpAdress;
            }
        }

        public string ClientName { get; set; }

        public string ServerAddress { get; set; }

        public string CrawlerStatus
        {
            get { return _crawlerStatus; }
            set { _crawlerStatus = value; RaisePropertyChanged("CrawlerStatus"); }
        }
    }
}