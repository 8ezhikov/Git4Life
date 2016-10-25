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
            AddSeedCommand = new RelayCommand(SaveSeed);
            DeleteSeedCommand  = new RelayCommand<Seed>(DeleteSeed);
            NewSeed = new Seed();
            SeedList = dataAccessService.GetSeeds();
        }

        public RelayCommand AddSeedCommand { get; set; }
        public RelayCommand<Seed> DeleteSeedCommand { get; set; }

        private SeedModel.IDataAccessService dataAccessService;
        private Seed _newSeed;
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

        private void SaveSeed()
        {
            if (dataAccessService.CreateSeed(NewSeed)>0)
            {
                SeedList.Add(NewSeed);
                MessageBox.Show("Successfully added new Seed!");
            }
        }

        private void DeleteSeed(Seed selectedSeed)
        {
          // SeedList.Remove(SelectedSeed);
            dataAccessService.DeleteSeed(selectedSeed);
        }

    }
}
