using System;
using System.Windows;
using CrawlerClient.CrawlerServer;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ConnectToServerCommand = new RelayCommand(ConnectToServer);
            StartTestCrawlingCommand = new RelayCommand(TestCrawl);
            CloseWindowCommand = new RelayCommand(CloseWindow);

            ClientName = $"Denis Test Crawler {DateTime.Now}";
            CrawlerStatus = "Waiting";
            ConnectionSingleton.Instance.InjectViewModel(this);
        }
        private void ConnectToServer()
        {
            var singleTone = ConnectionSingleton.Instance;

            var newPerson = new ClientCrawlerInfo
            {
                ClientName = ClientName,
                ServerIP = LocalIpAddress
            };

           var result =  singleTone.Connect(newPerson);
            if (result)
                CrawlerStatus = "Connected";

        }
        private void CloseWindow()
        {
            ConnectionSingleton.Instance.Disconnect();
        }
        private void TestCrawl()
        {
            var crawlerInstance = new CrawlerEngine();
            var seed = new SeedDTO {SeedDomainName = "http://webometrics.krc.karelia.ru/"};
            crawlerInstance.StartCrawlingProcess(new[] {seed});
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
        private  string _crawlerStatus;

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

        public  string CrawlerStatus
        {
            get { return _crawlerStatus; }
            set { _crawlerStatus = value; RaisePropertyChanged("CrawlerStatus"); }
        }
    }
}