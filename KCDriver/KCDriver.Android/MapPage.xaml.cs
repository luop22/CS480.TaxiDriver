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
using System.ComponentModel;

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
            KCApi.Properties.PropertyChanged += new PropertyChangedEventHandler(OnElementPropertyChanged);
        }

        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentRide":
                    UpdateText();
                    break;

                case "RideActive":
                    if (!KCApi.Properties.RideActive)
                    {
                        KCApi.Stop();
                        Navigation.PopAsync();
                        
                    }
                    break;
            }
        }

        //executes everytime the page appears.
        protected override void OnAppearing() {
            //if the authentication timer is null start the timer.
            if (authTimer == null) {
                SetTimer();
            }
            base.OnAppearing();

            
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
            else
            {
                var text = "The Ride has been canceled.";

                KCApi.Properties.RideActive = false;

                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                });

            }
        }

        public void ButtonCompleteRide(object sender, EventArgs e)
        {
            if (!KCApi.CompleteRide(KCApi.Properties.CurrentRide))
            {
                var text = "There was a problem completing the ride.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
            else
            {
                var text = "The Ride has been compleated.";
                KCApi.Properties.RideActive = false;

                Device.BeginInvokeOnMainThread(() => {
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                });
            }
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
            authTimer.Elapsed += CheckAuth;
            authTimer.AutoReset = false;
            authTimer.Enabled = true;
        }

        //Timer which checks if the driver is still authenticated if they arn't it kicks them back to the login page.
        public void CheckAuth(Object source, ElapsedEventArgs e) {
            if (!Driver_Id.authenticated && KCApi.Properties.RideActive) {
                Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                Device.BeginInvokeOnMainThread(() => {
                   KCApi.Stop();
                   Navigation.PopAsync();
                });
            }
            authTimer.Interval = 2000;
            authTimer.Start();
        }

        public async void UpdateText()
        {
            await Task.Run(() =>
            {
                KCApi.Properties.CurrentRide.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(KCApi.Properties.CurrentRide.ClientLat,
                                                                KCApi.Properties.CurrentRide.ClientLong)));

                Device.BeginInvokeOnMainThread(() =>
                {
                    RiderCardText.Text = KCApi.Properties.CurrentRide.ClientName + ": " + KCApi.Properties.CurrentRide.DisplayAddress;
                });
            });       
        }
    }
}