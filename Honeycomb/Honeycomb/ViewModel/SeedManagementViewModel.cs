using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Honeycomb.ViewModel
{
    public class SeedManagementViewModel : ViewModelBase
    {
        public SeedManagementViewModel()
        {
            AddSeedCommand = new RelayCommand<Seed>(SaveEmployee);
            NewSeed = new Seed();
        }

        public RelayCommand<Seed> AddSeedCommand { get; set; }

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
         
            
            //EmpInfo.EmpNo = _serviceProxy.CreateEmployee(emp);
            //if (EmpInfo.EmpNo != 0)
            //{
            //    Employees.Add(EmpInfo);
            //    RaisePropertyChanged("EmpInfo");
            //}
        }

    }
}
