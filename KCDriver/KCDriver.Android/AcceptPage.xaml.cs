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
        Ride ride;
        MapPage mapPage;

        public AcceptPage()
        {
            BindingContext = new AcceptUpdater();
            InitializeComponent();
            mapPage = new MapPage();
        }

        void Button_Clicked(object sender, EventArgs e)
        { 
            if (KCApi.AcceptNextRide(ride) 
                && KCApi.SetRideLocation(ride, KCApi.Properties.CurrentPosition.Latitude, KCApi.Properties.CurrentPosition.Longitude)) {
                //Start takes only a position, which will come from the database
                KCApi.Start(ride);
                Navigation.PushAsync(mapPage);
            }
        }
    }
}