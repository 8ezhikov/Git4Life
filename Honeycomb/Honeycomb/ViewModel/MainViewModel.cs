using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Honeycomb.Models;
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

        public MainViewModel()
        {
            StartCrawlingCommand = new RelayCommand(StartCrawling, () => true);
            StartTestCrawlingCommand = new RelayCommand(StartTestCrawling, () => true);

            ReadAllCommand = new RelayCommand(GetEmployees);
            ShowSeedWindowCommand = new RelayCommand(ShowSeedWindow);
            var uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            instance = new RemoteCrawlerService();
            host = new ServiceHost(instance, uri);
            host.Open();
            ClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();
            ClientCrawlers = instance.ConnectedClientCrawlers;

            TextBoxContent += "Starting Server...\n";
            TextBoxContent += "Press ENTER to stop chat service... \n";
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

        private void StartTestCrawling()
        {
            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            hoster.GiveTestInitialTasks();
        }

        private void StartCrawling()
        {
            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            hoster.GiveInitialTasks();
        }

        void ShowSeedWindow()
        {
            var window = new SeedManagement();
            window.Show();
        }

        void GetEmployees()
        {
            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            hoster.GiveInitialTasks();
        }
    }
}