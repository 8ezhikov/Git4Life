using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Honeycomb.ViewModel
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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        ServiceHost host;
        private RemoteCrawlerService instance;
        public RelayCommand ReadAllCommand { get; set; }
        public MainViewModel()
        {
            ShowPopUp = new RelayCommand(ShowPopUpExecute, () => true);
            ReadAllCommand = new RelayCommand(GetEmployees);
            var uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            instance = new RemoteCrawlerService();
            host = new ServiceHost(instance, uri);
            host.Open();
            ClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();
            ClientCrawlers = instance.ConnectedClientCrawlers;

        }

        private Seed _seedInfo;
        public Seed SeedInfo
        {
            get { return _seedInfo; }
            set
            {
                _seedInfo = value;
                RaisePropertyChanged("SeedInfo");
            }
        }

        void SaveSeed(Seed emp)
        {
            instance.SaveSeed(emp);

           // EmpInfo.EmpNo = _serviceProxy.CreateEmployee(emp);
           // if (EmpInfo.EmpNo != 0)
           // {
           //     Employees.Add(EmpInfo);
           //     RaisePropertyChanged("EmpInfo");
           // }
           ////     Employees.Add(EmpInfo);
           //     RaisePropertyChanged("SeedInfo");
           
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
        public ICommand ShowPopUp { get; private set; }

        public string TextBoxContent
        {
            get { return _textBoxContent; }
            set
            {
                RaisePropertyChanged("TextBoxContent");
                _textBoxContent = value;
            }
        }

        private void ShowPopUpExecute()
        {

            var hoster = ((RemoteCrawlerService) host.SingletonInstance);
            hoster.StartCrawling();
            //MessageBox.Show("Hello!");
        }

        void GetEmployees()
        {

            var hoster = ((RemoteCrawlerService)host.SingletonInstance);
            hoster.StartCrawling();
            //var tempData = new ClientCrawlerInfo("192.168.50.123", "Test 1");
            //var tempData1 = new ClientCrawlerInfo("192.161218.50.123", "Test 343 ");
            //ClientCrawlers.Add(tempData);
            //ClientCrawlers.Add(tempData1);
            //ClientCrawlers.Clear();
            //foreach (var item in instance.ConnectedClientCrawlers)
            //{
            //    ClientCrawlers.Add(item);
            //}
        }
    }
}