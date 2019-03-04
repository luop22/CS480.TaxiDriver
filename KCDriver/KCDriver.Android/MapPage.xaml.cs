using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Android.Widget;
using Plugin.CurrentActivity;
using System.Timers;
using System.ComponentModel;

namespace KCDriver.Droid {
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        //the timer which checks the ride queue.
        static System.Timers.Timer activeTimer;

        public MapPage ()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            KCApi.Properties.PropertyChanged += new PropertyChangedEventHandler(OnElementPropertyChanged);
        }

        /// <summary>
        /// Subscriber function to detect interesting property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentRide":
                    UpdateText();
                    break;
            }
        }

        protected override void OnAppearing() {
            //if the authentication timer is null start the timer.
            if (activeTimer == null) {
                SetTimer();
            }
            base.OnAppearing();

            if (KCApi.Properties.CameraOnDriver) 
            {
                KCApi.Properties.CameraOnDriver = false;
                ButtonSetDriverCameraLock(null, null);
            }
            else 
            {
                KCApi.Properties.CameraOnRider = false;
                ButtonSetRiderCameraLock(null, null);
            }
        }

        protected override void OnDisappearing() {
            //When the page dissapears the authentication timer is stoped.
            activeTimer.Stop();
            activeTimer = null;
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
                var text = "The Ride has been completed.";
                KCApi.Properties.RideActive = false;

                Device.BeginInvokeOnMainThread(() => {
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                });
            }
        }

        public void ButtonCallRide(object sender, EventArgs e)
        {

            DisplayAlert(KCApi.Properties.CurrentRide.ClientName + "'s Phone Number is:", 
                KCApi.Properties.CurrentRide.PhoneNum, "OK");
         
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

        /// <summary>
        /// Sets up the timer which checks if the ride is still active
        /// and driver authenticated.
        /// </summary>
        public void SetTimer() {
            // Create a timer with a two second interval.
            activeTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            activeTimer.Elapsed += CheckActive;
            activeTimer.AutoReset = false;
            activeTimer.Enabled = true;
        }

        //Timer which checks if the driver is still authenticated if they arn't it kicks them back to the login page.
        public void CheckActive(Object source, ElapsedEventArgs e) {
            if (!Driver_Id.authenticated && KCApi.Properties.RideActive) {
                Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                Device.BeginInvokeOnMainThread(() => {
                   KCApi.Stop();
                   Navigation.PopAsync();
                });
            }
            //If the ride is inactive then popback to the Accept page.
            else if (!KCApi.Properties.RideActive) {
                KCApi.Stop();
                Device.BeginInvokeOnMainThread(() => {
                    Navigation.PopAsync();
                });
            }
            else 
            {
                activeTimer.Interval = 16.66f;
                activeTimer.Start();
            }
        }

        /// <summary>
        /// Updates the address text when the ride is updated.
        /// </summary>
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