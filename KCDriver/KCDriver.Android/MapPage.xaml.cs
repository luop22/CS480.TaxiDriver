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
using System.Timers;

namespace KCDriver.Droid
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        //the timer which checks the ride queue.
        static System.Timers.Timer authTimer;

        public MapPage ()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        //executes everytime the page appears.
        protected override void OnAppearing() {
            //if the authentication timer is null start the timer.
            if (authTimer == null) {
                SetTimer();
            }
            base.OnAppearing();

            RiderCardText.Text = KCApi.Properties.CurrentRide.ClientName + ": " + KCApi.Properties.CurrentRide.DisplayAddress;
        }
        //executes everytime the page dissapears.
        protected override void OnDisappearing() {
            //When the page dissapears the authentication timer is stoped.
            authTimer.Stop();
            authTimer = null;
            base.OnDisappearing();
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

            DisplayAlert("Client Number", KCApi.Properties.CurrentRide.PhoneNum, "OK");
         
        }

        public void ButtonSetRiderCameraLock(object sender, EventArgs e)
        {

            KCApi.Properties.CameraOnRider = !KCApi.Properties.CameraOnRider;

            if (KCApi.Properties.CameraOnRider)
            {
                ButtonSetRiderCamera.BorderColor = Color.Yellow;
                ButtonSetDriverCamera.BorderColor = Color.Black;
            }
            else
            {
                ButtonSetRiderCamera.BorderColor = Color.Black;
            }

        }

        public void ButtonSetDriverCameraLock(object sender, EventArgs e)
        {

            KCApi.Properties.CameraOnDriver = !KCApi.Properties.CameraOnDriver;

            if (KCApi.Properties.CameraOnDriver)
            {
                ButtonSetDriverCamera.BorderColor = Color.Yellow;
                ButtonSetRiderCamera.BorderColor = Color.Black;
            }
            else
            {
                ButtonSetDriverCamera.BorderColor = Color.Black;
            }
        }


        public void SetTimer() {
            // Create a timer with a two second interval.
            authTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            authTimer.Elapsed += checkAuth;
            authTimer.AutoReset = false;
            authTimer.Enabled = true;
        }

        //Timer which checks if the driver is still authenticated if they arn't it kicks them back to the login page.
        public void checkAuth(Object source, ElapsedEventArgs e) {
            if (!Driver_Id.authenticated) {
                Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                Device.BeginInvokeOnMainThread(() => {
                   KCApi.Stop();
                   Navigation.PopAsync();
                });
            }
            authTimer.Interval = 2000;
            authTimer.Start();
        }
    }
}