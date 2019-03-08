﻿using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Android.Widget;
using Plugin.CurrentActivity;
using System.Timers;
using System.ComponentModel;
using System.Threading;

namespace KCDriver.Droid {
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        //the timer which checks the ride queue.
        static System.Timers.Timer activeTimer;

        static readonly object buttonLock = new object();

        /// <summary>
        /// Starts navigation and initializes the map page and starts the timer 
        /// to check if the there is an active ride.
        /// </summary>
        public MapPage ()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            KCApi.Properties.PropertyChanged += new PropertyChangedEventHandler(OnElementPropertyChanged);

            activeTimer = new System.Timers.Timer(16.66f);
            activeTimer.Elapsed += CheckActive;
            activeTimer.Enabled = false;
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
                case "CurrentPosition":
                    if (KCApi.Properties.CurrentPosition.Latitude != 0 || KCApi.Properties.CurrentPosition.Longitude != 0)
                    {
                        if (ButtonSetDriverCamera.BorderColor == Color.Gray)
                            Device.BeginInvokeOnMainThread(() => {
                                ButtonSetDriverCamera.BorderColor = Color.Black;
                            });
                    }
                    else
                    {
                        if (ButtonSetDriverCamera.BorderColor == Color.Black)
                            Device.BeginInvokeOnMainThread(() => {
                                ButtonSetDriverCamera.BorderColor = Color.Gray;
                            });

                        if (KCApi.Properties.CameraOnDriver)
                        {
                            KCApi.Properties.CameraOnRider = false;
                            Device.BeginInvokeOnMainThread(() => {
                                ButtonSetRiderCameraLock(null, null);
                            });
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// When the map page appears the current posstion is retrived and the camera is adjusted.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            KCApi.Properties.State = KCProperties.AppState.Map;
            UpdateText();

            if ( (KCApi.Properties.CurrentPosition.Latitude != 0 || KCApi.Properties.CurrentPosition.Longitude != 0)
                    && KCApi.Properties.CameraOnDriver) 
            {
                KCApi.Properties.CameraOnDriver = false;
                Device.BeginInvokeOnMainThread(() => {
                    ButtonSetDriverCameraLock(null, null);
                });
            }
            else 
            {
                KCApi.Properties.CameraOnRider = false;
                Device.BeginInvokeOnMainThread(() => {
                    ButtonSetRiderCameraLock(null, null);

                    if (KCApi.Properties.CurrentPosition.Latitude != 0 || KCApi.Properties.CurrentPosition.Longitude != 0)
                        ButtonSetDriverCamera.BorderColor = Color.Black;
                    else
                        ButtonSetDriverCamera.BorderColor = Color.Gray;
                });
            }

            activeTimer.Start();
        }

        /// <summary>
        /// When the map page disappears the active timer stops.
        /// </summary>
        protected override void OnDisappearing() {
            base.OnDisappearing();

            //When the page dissapears the authentication timer is stoped.
            activeTimer.Stop();
        }

        /// <summary>
        /// When the back button is pressed the ride is canceled.
        /// </summary>
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            ButtonDisable();

            if (KCApi.Properties.State == KCProperties.AppState.Map)
                Device.BeginInvokeOnMainThread(() => {
                    ButtonCancelRide(null, null);
                });

            ButtonEnable();
            return false;
        }

        /// <summary>
        /// When the cancel button is pressed rideactive is set to false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonCancelRide(object sender, EventArgs e)
        {
            ButtonDisable();

            if (!KCApi.CancelRide(KCApi.Properties.CurrentRide))
            {
                var text = "There was a problem with cancellation.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
            else
            {
                var text = "The Ride has been canceled.";

                KCApi.Properties.RideActive = false;
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }

            ButtonEnable();
        }
        
        /// <summary>
        /// When a ride is completed rideactive is set to false and the ride is compleated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonCompleteRide(object sender, EventArgs e)
        {
            ButtonDisable();

            if (!KCApi.CompleteRide(KCApi.Properties.CurrentRide))
            {
                var text = "There was a problem completing the ride.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }
            else
            {
                var text = "The Ride has been completed.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();

                KCApi.Properties.RideActive = false;
            }

            ButtonEnable();
        }


        /// <summary>
        /// Sends an alert which displays the clients phone number 
        /// and provides a call button which launches the phone app if the device is able to make calls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonCallRide(object sender, EventArgs e)
        {
            ButtonDisable();
            //DisplayAlert(KCApi.Properties.CurrentRide.ClientName + "'s Phone Number is:", KCApi.Properties.CurrentRide.PhoneNum, "OK");

            var alertDialog = new Android.App.AlertDialog.Builder(CrossCurrentActivity.Current.Activity);
            alertDialog.SetTitle(KCApi.Properties.CurrentRide.ClientName + "'s Phone Number");
            alertDialog.SetMessage("(" + KCApi.Properties.CurrentRide.PhoneNum.Substring(0,3) + ") " 
                + KCApi.Properties.CurrentRide.PhoneNum.Substring(3, 3) + "-" + KCApi.Properties.CurrentRide.PhoneNum.Substring(6));
            alertDialog.SetPositiveButton("Call", (senderad, args) =>
            {
                Device.OpenUri(new Uri("tel:" + KCApi.Properties.CurrentRide.PhoneNum));
            });
            alertDialog.SetNegativeButton("Cancel", (senderad, args) =>
            {
                //Does not want to call
            });
            alertDialog.Create().Show();

            ButtonEnable();
        }

        /// <summary>
        /// Sets the camera to lock on the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonSetRiderCameraLock(object sender, EventArgs e)
        {
            ButtonDisable();

            KCApi.Properties.CameraOnRider = !KCApi.Properties.CameraOnRider;

            if (KCApi.Properties.CameraOnRider)
            {
                ButtonSetRiderCamera.BorderColor = Color.Yellow;

                if (KCApi.Properties.CurrentPosition.Latitude != 0 || KCApi.Properties.CurrentPosition.Longitude != 0)
                    ButtonSetDriverCamera.BorderColor = Color.Black;
                else
                    ButtonSetDriverCamera.BorderColor = Color.Gray;
            }
            else
            {
                ButtonSetRiderCamera.BorderColor = Color.Black;
            }

            ButtonEnable();
        }

        /// <summary>
        /// Sets the camera to lock on to the driver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonSetDriverCameraLock(object sender, EventArgs e)
        {
            ButtonDisable();

            if (KCApi.Properties.CurrentPosition.Latitude != 0 || KCApi.Properties.CurrentPosition.Longitude != 0)
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
            else
            {
                var text = "Location unknown. Please enable GPS or reenter service.";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
            }

            ButtonEnable();
        }

        /// <summary>
        /// Timer which checks if the driver is still authenticated if they aren't it kicks them back to the login page.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void CheckActive(Object source, ElapsedEventArgs e) {
            Task.Run(() => { 
                try
                {
                    lock (KCApi.Properties.StateLock)
                    {
                        if (KCApi.Properties.State == KCProperties.AppState.Map)
                        {
                            bool pop = false;
                            if (!Driver_Id.authenticated && KCApi.Properties.RideActive)
                            {
                                // Pop us back to the main page!
                                Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                                pop = true;
                            }
                            //If the ride is inactive then pop back to the Accept page.
                            else if (!KCApi.Properties.RideActive && KCApi.Properties.RenderReady)
                            {
                                var text = "Rider has cancelled ride.";
                                Device.BeginInvokeOnMainThread(() => {
                                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                                });

                                pop = true;
                            }
                            else if (KCApi.Properties.NetState == KCProperties.NetworkState.Disconnected)
                            {
                                // Back to the main page
                                Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                                var text = "Internet connection lost.";
                                Device.BeginInvokeOnMainThread(() => {
                                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                                });

                                pop = true;
                            }

                            if (pop)
                            {
                                KCApi.Stop();
                                activeTimer.Stop();

                                KCApi.Properties.State = KCProperties.AppState.Transitioning;

                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await Navigation.PopAsync();
                                });
                            }
                            else
                            {
                                activeTimer.Interval = 100.0f; // Can be optimized
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    KCApi.OutputException(ex);
                }
            });
        }

        /// <summary>
        /// Updates the address text when the ride is updated.
        /// </summary>
        public async void UpdateText()
        {
            if (KCApi.Properties.State == KCProperties.AppState.Map)
                await Task.Run(() =>
                {
                    while (KCApi.Properties.CurrentRide == null) ;

                    KCApi.Properties.CurrentRide.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(KCApi.Properties.CurrentRide.ClientLat,
                                                                        KCApi.Properties.CurrentRide.ClientLong)));

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        RiderCardText.Text = KCApi.Properties.CurrentRide.ClientName + ": " + KCApi.Properties.CurrentRide.DisplayAddress;
                    });
                });       
        }

        /// <summary>
        /// Disables the use of all buttons
        /// </summary>
        private void ButtonDisable() {

            lock(buttonLock)
            {
                completeBtn.IsEnabled = false;
                cancelBtn.IsEnabled = false;
                phoneBtn.IsEnabled = false;
                ButtonSetDriverCamera.IsEnabled = false;
                ButtonSetRiderCamera.IsEnabled = false;
            }
        }


        /// <summary>
        /// Enables the use of all buttons
        /// </summary>
        private void ButtonEnable() {

            lock(buttonLock)
            {
                completeBtn.IsEnabled = true;
                cancelBtn.IsEnabled = true;
                phoneBtn.IsEnabled = true;
                ButtonSetDriverCamera.IsEnabled = true;
                ButtonSetRiderCamera.IsEnabled = true;
            }
        }
    }
}