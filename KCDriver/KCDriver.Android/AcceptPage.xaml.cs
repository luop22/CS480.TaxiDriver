using Android.Widget;
using Plugin.CurrentActivity;
using Plugin.Geolocator;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace KCDriver.Droid {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AcceptPage : ContentPage
    {
        MapPage mapPage;
        object buttonLock;
        static System.Timers.Timer updater;

        public AcceptPage()
        {
            InitializeComponent();
            mapPage = new MapPage();
            buttonLock = new object();

            updater = new System.Timers.Timer(500);
            updater.Elapsed += Timer;
            updater.Enabled = false;
        }

        //executes everytime the page appears.
        protected override async void OnAppearing() {
            base.OnAppearing();

            if (KCApi.Properties.State != KCProperties.AppState.Accept)
            {
                KCApi.Properties.State = KCProperties.AppState.Accept;

                KCApi.Properties.CurrentPosition = await KCApi.GetCurrentPosition();

                // Location is not set
                if (KCApi.Properties.CurrentPosition.Latitude == 0 && KCApi.Properties.CurrentPosition.Longitude == 0)
                {
                    var text = "GPS signal lost. Please reenable or reenter service.";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                }
                // Driver is no longer authenticated. Send back to sign in
                else if (!Driver_Id.authenticated)
                {
                    var text = "Authentication Failure";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();

                    if (Navigation != null && Navigation.NavigationStack.Count > 1)
                        await Navigation.PopAsync();
                }
                // Driver has entered the Accept screen for the first time since logging in
                else if (!KCApi.Properties.RideActive && KCApi.Properties.CurrentRide == null)
                {
                    Ride ride = new Ride();

                    if (KCApi.RecoveryCheck(ride) && KCApi.SetRideLocation(ride))
                    {
                        ride.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(ride.ClientLat, ride.ClientLong)));
                        KCApi.Start(ride);
                        await Navigation.PushAsync(mapPage);
                    }
                }

                updater.Start();
            }
        }
        //executes everytime the page disappears.
        protected override void OnDisappearing() {
            //When the page disappears the update timer is stopped.
            updater.Stop();

            base.OnDisappearing();
        }

        /// <summary>
        /// Function to start map and get ride location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button_Clicked(object sender, EventArgs e)
        {
            lock (buttonLock) 
            {
                StatusColor.IsEnabled = false;

                Ride ride = new Ride();
                KCApi.Properties.CurrentPosition = Task.Run(async () => await KCApi.GetCurrentPosition()).Result;

                if (KCApi.Properties.CurrentPosition.Latitude == 0 && KCApi.Properties.CurrentPosition.Longitude == 0)
                {
                    var text = "GPS signal lost. Please reenable or reenter service.";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                }
                else if (!KCApi.Properties.RideActive && KCApi.AcceptNextRide(ride)
                            && KCApi.SetRideLocation(ride, KCApi.Properties.CurrentPosition.Latitude, KCApi.Properties.CurrentPosition.Longitude)) {
                    //Start takes only a position, which will come from the database
                    ride.SetDisplayAddress(KCApi.GetAddressFromPosition(new Position(ride.ClientLat, ride.ClientLong)));
                    KCApi.Start(ride);
                    Navigation.PushAsync(mapPage);
                }
                else if (!Driver_Id.authenticated) {
                    var text = "Authentication Failure";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();

                    lock (KCApi.Properties.StateLock)
                    {
                        if (KCApi.Properties.State == KCProperties.AppState.Accept && Navigation.NavigationStack.Count > 1)
                            Navigation.PopAsync();
                    }
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

                StatusColor.IsEnabled = true;
            }
        }

        /// <summary>
        /// Timer function which updates the driver if there are any rides in the queue.
        /// </summary>
        private void Timer(Object source, ElapsedEventArgs e) {

            string status = KCApi.CheckQueue();
            Device.BeginInvokeOnMainThread(() => {
                Status.Text = status;

                if(status.Equals("Rides are available"))
                {
                    StatusColor.BackgroundColor = Color.Green;
                } else
                {
                    StatusColor.BackgroundColor = Color.Gray;
                }
            });

            updater.Interval = 500; // Can optimize to reduce interval depending on time taken
        }
    }
}