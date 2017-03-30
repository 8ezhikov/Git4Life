using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            hoster.GiveTestInitialTasks();
        }

        private void StartCrawling()
        {

            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            if (IsRobin)
            {
                hoster.CrawlMode = CrawlingMethod.RoundRobin;
            }
            if (IsBatch)
            {
                hoster.CrawlMode = CrawlingMethod.BatchMode;
            }
            Task.Run(() => hoster.GiveInitialTasks());

        }

        void ShowSeedWindow()
        {
            var window = new SeedManagement();
            window.Show();
        }

        void LoadBatchAllocation()
        {
            var file = new OpenFileDialog();
            file.InitialDirectory = "c:\\";
            file.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            file.FilterIndex = 2;
            file.RestoreDirectory = true;
            Log.Error(new Exception(), "hey Seq");
            Log.CloseAndFlush();
        }
    }
}