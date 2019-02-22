using Android.Widget;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AcceptPage : ContentPage
    {
        MapPage mapPage;

        public AcceptPage()
        {
            BindingContext = new AcceptUpdater();
            InitializeComponent();
            mapPage = new MapPage();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            Ride ride = KCApi.Properties.CurrentRide;
            if (KCApi.AcceptNextRide(ride) 
                && KCApi.SetRideLocation(ride, KCApi.Properties.CurrentPosition.Latitude, KCApi.Properties.CurrentPosition.Longitude)) {
                //Start takes only a position, which will come from the database
                KCApi.Start(ride);
                Navigation.PushAsync(mapPage);
            }
            else
            {
                var text = "Accept ride failed.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
        }
    }
}