using System.Collections.ObjectModel;
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
        public MainViewModel()
        {
            ShowPopUp = new RelayCommand(ShowPopUpExecute, () => true);
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

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

        private void ShowPopUpExecute()
        {
            MessageBox.Show("Hello!");
        }
    }
}