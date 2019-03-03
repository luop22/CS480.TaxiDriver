using System;
using System.Collections.Generic;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using Plugin.Geolocator;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using Xamarin.Forms;
using System.Net;
using Newtonsoft.Json;

namespace KCDriver.Droid
{
    static partial class KCApi
    {
        public static KCProperties Properties = new KCProperties();
        private static System.Timers.Timer updatePositionTimer;
        private static System.Timers.Timer updateCameraTimer;

        private static readonly object exceptionsLock = new object();
        private static List<Exception> exceptions = new List<Exception>();

        // Set default values and start timers
        public static void Initialize()
        {
            Properties.MapReady = false;
            Properties.RenderReady = false;
            Properties.CameraOnDriver = true;
            Properties.CameraOnRider = false;
            Properties.RideActive = false;

            Properties.CurrentRide = new Ride();

            // The timer automatically updates the camera and position every interval.
            updatePositionTimer = new System.Timers.Timer(500.0f);
            updatePositionTimer.Elapsed += (o,e) => Task.Factory.StartNew( () => UpdatePosition(o,e));

            updateCameraTimer = new System.Timers.Timer(16.66f);
            updateCameraTimer.Elapsed += (o, e) => Task.Factory.StartNew(() => UpdateCamera(o, e));
        }

        /// A timer calls this 10 times per second. Updates the db on where the driver is.
        public static void UpdatePosition(Object source, ElapsedEventArgs e)
        {
            if (!Properties.MapReady || !Properties.RenderReady)
                return;

            updatePositionTimer.Interval = 500.0f;
            updatePositionTimer.Start();

            ThreadPool.QueueUserWorkItem(o => {
                //if the current position is 0,0 then don't send the position.
                if (Properties.CurrentPosition.Latitude != 0 && Properties.CurrentPosition.Longitude != 0) {
                    //set the drivers current position.
                    if (!SetDriverLocation(Properties.CurrentPosition.Latitude, Properties.CurrentPosition.Longitude)) {
                        Debug.WriteLine("Setting driver location failed.");
                    }

                    Ride temp = new Ride(Properties.CurrentRide);

                    if (!SetRideLocation(temp, Properties.CurrentPosition.Latitude, Properties.CurrentPosition.Longitude)) {
                        Properties.RideActive = false;
                    } else {
                        // Only update current ride if needed, since it will trigger a UI update.
                        if (temp.ClientLat != Properties.CurrentRide.ClientLat
                        || temp.ClientLong != Properties.CurrentRide.ClientLong)
                            Properties.CurrentRide = temp;
                    }
                }
            }); 
        }

        public static void UpdateCamera(Object source, ElapsedEventArgs e)
        {
            if (!Properties.MapReady || !Properties.RenderReady)
                return;

            Properties.CurrentPosition = GetCurrentPosition();

            updateCameraTimer.Interval = 16.66f;
            updateCameraTimer.Start();

            //Keeps the camera locked on where it is supposed to be.
            Device.BeginInvokeOnMainThread( () =>
            {
                Position temp = new Position();

                if (Properties.CameraOnDriver)
                    temp = KCApi.Properties.CurrentPosition;
                else if (Properties.CameraOnRider)
                    temp = new Position(Properties.CurrentRide.ClientLat, Properties.CurrentRide.ClientLong);

                if (Properties.CameraOnDriver || Properties.CameraOnRider)
                {
                    if (temp != null)
                        KCApi.Properties.Renderer.MoveCameraTo(temp.Latitude, temp.Longitude);

                    KCApi.Properties.Map.HasScrollEnabled = false;
                }
                else KCApi.Properties.Map.HasScrollEnabled = true;
            });
        }

        // Starts navigation functions. Takes riders position and destination address string.
        public static void Start(Ride ride)
        {
            Properties.CurrentRide = ride;
            updatePositionTimer.Start();
            updateCameraTimer.Start();

            Properties.RideActive = true;
        }

        // Stops navigation, does cleanup, and outputs recorded exceptions in debug.
        public static void Stop()
        {
            updatePositionTimer.Stop();
            updateCameraTimer.Stop();

            if (Properties.RideActive)
                Properties.RideActive = false;

            //Debug
            lock(exceptionsLock)
            {
                Debug.WriteLine("------------------------- Exception Output ------------------------");
                foreach (Exception e in exceptions)
                {
                    Debug.WriteLine("Error ------");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    Debug.WriteLine(e.TargetSite);
                    Debug.WriteLine("End --------");

                }
                Debug.WriteLine("------------------------- End -------------------------------------");
            }
            
        }

        // Plugin example function to get the current position.
        public static Position GetCurrentPosition()
        {
            Plugin.Geolocator.Abstractions.Position position = new Plugin.Geolocator.Abstractions.Position(0, 0);

            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;

                position = Task.Run(async () => await locator.GetLastKnownLocationAsync()).Result;

                if (position != null)
                {
                    //got a cahched position, so let's use it.
                    return new Position(position.Latitude, position.Longitude);
                }

                if (!locator.IsGeolocationAvailable || !locator.IsGeolocationEnabled)
                {
                    //not available or enabled
                     throw new Exception("Geolocation not available or not enabled.");
                }

                position = Task.Run(async () => await locator.GetPositionAsync(TimeSpan.FromSeconds(20), null, true)).Result;
            }
            catch (Exception e)
            {
                OutputException(e);
            }

            return new Position(position.Latitude, position.Longitude);
        }

        public static string GetAddressFromPosition(Position pos)
        {
            try
            {
                /* Prod API Key: AIzaSyDku7O-5lR0g8fOx04lPSAPA5T8-JmK1a0 - Needs to be updated with Geocoding API as of 2/28/19 */
                /* Developer: AIzaSyAgdPpZhmK2UGsVKkJ5UWGp-w46aSt2Npo */
                string request = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + pos.Latitude + "," + pos.Longitude
                    + "&key=AIzaSyAgdPpZhmK2UGsVKkJ5UWGp-w46aSt2Npo";

                WebClient client = new WebClient();
                string s = client.DownloadString(request);

                var obj = JsonConvert.DeserializeObject<GoogleRevGeocoderResponse>(s);

                return obj.results[0].formatted_address;
            }
            catch (Exception e)
            {
                OutputException(e);
                return "Error";
            }
            
        }

        public static void OutputException(Exception e)
        {
            lock(exceptionsLock)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                exceptions.Add(e);
            }
        }
    }
}