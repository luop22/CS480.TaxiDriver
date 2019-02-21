using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


//https://www.youtube.com/watch?v=vQBrMoh_HSQ
namespace KCDriver.Droid {
    class AcceptUpdater : INotifyPropertyChanged {

        string ride_Status = "Next Available Job";
        public string Ride_Status {
            get { return ride_Status; }
            set { ride_Status = value;
                OnPropertyChange(nameof(ride_Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChange(string ride_Status) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ride_Status));
        }
    }
}