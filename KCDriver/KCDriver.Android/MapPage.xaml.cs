using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Android.Widget;
using Plugin.CurrentActivity;

namespace KCDriver.Droid
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
		public MapPage ()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override bool OnBackButtonPressed()
        {
            KCApi.CancelRide(KCApi.Properties.CurrentRide);
            KCApi.Stop();
            return false;
        }

        public void ButtonCancelRide(object sender, EventArgs e)
        {
            if (!KCApi.CancelRide(KCApi.Properties.CurrentRide))
            {
                var text = "There was a problem with cancellation.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
            else Navigation.PopAsync();
        }

        public void ButtonCompleteRide(object sender, EventArgs e)
        {
            if (!KCApi.CompleteRide(KCApi.Properties.CurrentRide))
            {
                var text = "There was a problem completing the ride.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
            else Navigation.PopAsync();
        }

        public void ButtonCallRide(object sender, EventArgs e)
        {
            //KCApi.Properties.CurrentRide.CallClient();
        }
    }
}