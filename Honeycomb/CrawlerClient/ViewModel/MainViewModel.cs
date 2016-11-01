using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Honeycomb;

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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ConnectToServerCommand = new RelayCommand(ConnectToServer);
            StartTestCrawlingCommand = new RelayCommand(TestCrawl);
            ClientName = $"Denis Test Crawler {DateTime.Now}";

        }
        private void ConnectToServer()
        {
            var singleTone = ConnectionSingleton.Instance;

            var newPerson = new ClientCrawlerInfo
            {
                ClientName = ClientName,
                ServerIP = LocalIpAddress
            };

            singleTone.Connect(newPerson);

        }

        private void ConnectToServer()
        {
            var crawlerInstance = new CrawlerEngine();
            crawlerInstance.StartCrawlingProcess("http://webometrics.krc.karelia.ru/");
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

      
    }
}