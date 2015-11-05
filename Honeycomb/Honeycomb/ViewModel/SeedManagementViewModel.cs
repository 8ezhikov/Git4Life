using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Honeycomb.Services;

namespace Honeycomb.ViewModel
{
    public class SeedManagementViewModel : ViewModelBase
    {
        public SeedManagementViewModel(SeedModel.IDataAccessService servPxy)
        {
            dataAccessService = servPxy;
            AddSeedCommand = new RelayCommand<Seed>(SaveEmployee);
            DeleteSeedCommand  = new RelayCommand<Seed>(DeleteSeed);
            NewSeed = new Seed();
            SeedList = dataAccessService.GetSeeds();
        }

        public RelayCommand<Seed> AddSeedCommand { get; set; }
        public RelayCommand<Seed> DeleteSeedCommand { get; set; }

        private SeedModel.IDataAccessService dataAccessService;
        Seed _newSeed;
        public Seed NewSeed
        {
            get { return _newSeed; }
            set
            {
                _newSeed = value;
                RaisePropertyChanged("NewSeed");
            }
        }

        private ObservableCollection<Seed> _seedList;
        public ObservableCollection<Seed> SeedList
        {
            get { return _seedList; }
            set
            {
                _seedList = value;
                RaisePropertyChanged("SeedList");
            }
        }

        void SaveEmployee(Seed emp)
        {
            if (dataAccessService.CreateSeed(NewSeed)>0)
            {
                SeedList.Add(NewSeed);
                MessageBox.Show("Successfully added new Seed!");
            }
        }

        void DeleteSeed(Seed selectedSeed)
        {
          // SeedList.Remove(SelectedSeed);
            dataAccessService.DeleteSeed(selectedSeed);
        }

    }
}
