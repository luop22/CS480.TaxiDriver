﻿using Android.Widget;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AcceptPage : ContentPage
    {
        MapPage mapPage;
        object buttonLock;
        //the timer which checks the ride queue.
        static System.Timers.Timer updater;

        public AcceptPage()
        {
            InitializeComponent();
            mapPage = new MapPage();
            buttonLock = new object();
        }

        //executes everytime the page appears.
        protected override void OnAppearing() {

            Ride ride = new Ride();
            //if the driver already has a ride
            if (KCApi.RecoveryCheck(ride) && KCApi.SetRideLocation(ride, KCApi.Properties.CurrentPosition.Latitude, KCApi.Properties.CurrentPosition.Longitude)) {
                ride.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(ride.ClientLat, ride.ClientLong)));
                KCApi.Start(ride);
                Navigation.PushAsync(mapPage);
            } else if (!Driver_Id.authenticated) {
                var text = "Authentication Failure";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                Navigation.PopAsync();
            }


            //if the update timer is null start the timer.
            if (updater == null) {

                SetTimer();
            }
            base.OnAppearing();
        }
        //executes everytime the page dissapears.
        protected override void OnDisappearing() {
            //When the page dissapears the update timer is stoped.
            updater.Stop();
            updater = null;
            base.OnDisappearing();
        }

        //Is called when the accept button is clicked.
        void Button_Clicked(object sender, EventArgs e)
        {
            lock(buttonLock)
            {
                //c
                Ride ride = new Ride();
                KCApi.Properties.CurrentPosition = KCApi.GetCurrentPosition();

                if (!KCApi.Properties.RideActive && KCApi.AcceptNextRide(ride) 
                    && KCApi.SetRideLocation(ride, KCApi.Properties.CurrentPosition.Latitude, KCApi.Properties.CurrentPosition.Longitude)) {
                    //Start takes only a position, which will come from the database
                    ride.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(ride.ClientLat, ride.ClientLong)));
                    KCApi.Start(ride);
                    Navigation.PushAsync(mapPage);
                }
                else if (!Driver_Id.authenticated) {
                    var text = "Authentication Failure";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                    Navigation.PopAsync();
                }
                else if (Status.Text == "No available rides")
                {
                    var text = "No available rides.";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                }
                else
                {
                    var text = "Accept ride failed.";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                }
            }
        }

        public void SetTimer() {
            // Create a timer with a two second interval.
            updater = new System.Timers.Timer(1);
            // Hook up the Elapsed event for the timer. 
            updater.Elapsed += Timer;
            updater.AutoReset = false;
            updater.Enabled = true;
        }

        //timer function which updates the driver if there are any rides in the queue.
        private void Timer(Object source, ElapsedEventArgs e) {

            string status = KCApi.CheckQueue();
            Device.BeginInvokeOnMainThread(() => {
                Status.Text = status;
            });
            updater.Interval = 500;
            updater.Start();
        }
    }
}