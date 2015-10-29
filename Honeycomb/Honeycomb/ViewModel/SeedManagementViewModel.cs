using System;
using System.Collections.Generic;
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
            NewSeed = new Seed();
        }

        public RelayCommand<Seed> AddSeedCommand { get; set; }
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

        void SaveEmployee(Seed emp)
        {
            if (dataAccessService.CreateSeed(NewSeed)>0)
            {
                MessageBox.Show("Successfully added new Seed!");
            }
                
          
        }

    }
}
