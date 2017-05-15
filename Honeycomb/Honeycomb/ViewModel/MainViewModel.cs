using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CsvHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Honeycomb.Models;
using Microsoft.Win32;
using Serilog;

namespace Honeycomb.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        private ServiceHost host;

        private RemoteCrawlerService instance;
        public RelayCommand ReadAllCommand { get; set; }
        public RelayCommand ShowSeedWindowCommand { get; set; }
        public RelayCommand StartCrawlingCommand { get; private set; }
        public RelayCommand StartTestCrawlingCommand { get; private set; }

        public bool IsRobin { get; set; }
        public bool IsBatch { get; set; }
        public bool IsMixed { get; set; }

        public int SeedNumber { get; set; }
     
        public MainViewModel()
        {
            StartCrawlingCommand = new RelayCommand(StartCrawling, () => true);
            StartTestCrawlingCommand = new RelayCommand(StartTestCrawling, () => true);

            ReadAllCommand = new RelayCommand(LoadBatchAllocation);
            ShowSeedWindowCommand = new RelayCommand(ShowSeedWindow);
            var uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            instance = new RemoteCrawlerService(this);
            host = new ServiceHost(instance, uri);
            host.Open();
            ClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();
            ClientCrawlers = instance.ConnectedClientCrawlers;
            AppendTextToConsole("Starting Server...");
            IsRobin = true;
            SeedNumber = 99;

        }

        private string _textBoxContent;

        ObservableCollection<ClientCrawlerInfo> _clientCrawlers;

        public ObservableCollection<ClientCrawlerInfo> ClientCrawlers
        {
            get { return _clientCrawlers; }
            set
            {
                _clientCrawlers = value;
                RaisePropertyChanged("ClientCrawlers");
            }
        }

        public string TextBoxContent
        {
            get { return _textBoxContent; }
            set
            {
                RaisePropertyChanged("TextBoxContent");
                _textBoxContent = value;
            }
        }

        public void AppendTextToConsole(string messageToAppend)
        {
            TextBoxContent = TextBoxContent + messageToAppend;
            TextBoxContent += Environment.NewLine;
        }

        private void StartTestCrawling()
        {
            var hoster = ((RemoteCrawlerService) host.SingletonInstance);
            hoster.GiveTestInitialTasks();
        }

        private void StartCrawling()
        {

            var hoster = ((RemoteCrawlerService) host.SingletonInstance);
            if (IsRobin)
            {
                hoster.CrawlMode = CrawlingMethod.RoundRobin;
            }
            else if (IsBatch)
            {
                var alloc = TestForBatch();
                if (alloc == null)
                {
                    MessageBox.Show("Seed to batch allocation is wrong!");
                    return;
                }

                hoster.CrawlMode = CrawlingMethod.BatchMode;
                hoster.SeedsByBatchAllocation = alloc;
            }
            else if (IsMixed)
            {
                var alloc = TestForBatch();
                if (alloc == null)
                {
                    MessageBox.Show("Seed to batch allocation is wrong!");
                    return;
                }

                hoster.CrawlMode = CrawlingMethod.MixedMode;
                hoster.SeedsByBatchAllocation = alloc;
            }

            Task.Run(() => hoster.GiveInitialTasks());

        }

        void ShowSeedWindow()
        {
            var window = new SeedManagement();
            window.Show();
        }

        private Dictionary<int, ConcurrentStack<int>> TestForBatch()
        {
            try
            {
                var dbCont = new Crawler_DBEntities();
               var seedsByBatch = new Dictionary<int, ConcurrentStack<int>>();
                var seedCounter = 0;
                foreach (var uiCrawler in ClientCrawlers)
                {
                    var crawlerNum = uiCrawler.BatchNumber;
                    var dbAlloc = dbCont.PredefinedJobs.FirstOrDefault(_ => _.ClientId == crawlerNum);
                    if (dbAlloc == null)
                    {
                        return null;
                    }
                    var batchSeeds = dbAlloc.SeedIds.Split(',').Select(int.Parse).Reverse().ToList();
                    var seedStack = new ConcurrentStack<int>(batchSeeds);
                    seedCounter += batchSeeds.Count;
                    seedsByBatch.Add(crawlerNum, seedStack);
                }

                if (seedCounter != SeedNumber)
                {
                    return null;
                }
                return seedsByBatch;
            }
            catch (Exception)
            {

                return null;
            }
            
        }
        void LoadBatchAllocation()
        {
            TestForBatch();
            //var file = new OpenFileDialog();
            //file.InitialDirectory = "c:\\";
            //file.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //file.FilterIndex = 2;
            //file.RestoreDirectory = true;

            //if (file.ShowDialog() == true)
            //{
            //    using (TextReader textReader = File.OpenText(file.FileName))
            //    {
            //        var csv = new CsvReader(textReader);
            //        while (csv.Read())
            //        {
            //            var ClientNumber = csv.GetField<int>(0);
            //            for (int i = 0; i < 100; i++)
            //            {
            //                int intField;
            //                if (!csv.TryGetField(0, out intField))
            //                {
            //                    // Do something when it can't convert.
            //                }
            //            }
            //            var stringField = csv.GetField<string>(1);
            //            var boolField = csv.GetField<bool>("HeaderName");
            //        }
            //    }

            //}
            Log.Error(new Exception(), "hey Seq");
            Log.CloseAndFlush();
        }
    }
}