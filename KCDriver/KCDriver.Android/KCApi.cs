using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using Plugin.Geolocator;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using Android.Gms.Maps.Model;
using KCDriver.Droid;
using Xamarin.Forms;

namespace KCDriver.Droid
{
    static partial class KCApi
    {
        public static KCProperties Properties = new KCProperties();
        private static System.Timers.Timer updatePositionTimer;
        private static List<Exception> exceptions = new List<Exception>();

        // Set default values and start timers
        public static void Initialize()
        {
            Properties.MapReady = false;
            Properties.RenderReady = false;
            Properties.RouteCoordinates = new List<Position>();
            Properties.CurrentRide = new Ride();

            // The timer automatically updates the camera and position every interval.
            updatePositionTimer = new System.Timers.Timer(100.0f);
            updatePositionTimer.Elapsed += (o,e) => Task.Factory.StartNew( () => UpdatePosition(o,e));
        }

        // A timer calls this 60 times per second. It checks for updates in position and updates the route list to reflect when 
        // the driver has gone farther than a certain point still in the que.
        public static void UpdatePosition(Object source, ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => {

                updatePositionTimer.Stop();
                Position p = GetCurrentPosition();

                if (!SetDriverLocation(p.Latitude, p.Longitude))
                {
                    Debug.WriteLine("Setting driver location failed.");
                }

                // TODO: Check if rider has cancelled here

                //Keeps the camera locked on the user
                /*Device.BeginInvokeOnMainThread(() =>
                {
                    Position temp = KCApi.Properties.CurrentPosition;
                    KCApi.Properties.Renderer.AnimateCameraTo(temp.Latitude, temp.Longitude);
                });*/

                updatePositionTimer.Interval = 100.0f;
                updatePositionTimer.Start();
            });
        }

        // Starts navigation functions. Takes riders position and destination address string.
        public static void Start(Ride ride)
        {
            Properties.CurrentRide = ride;
            updatePositionTimer.Enabled = true;
        }

        // Stops navigation, does cleanup, and outputs recorded exceptions in debug.
        public static void Stop()
        {
            updatePositionTimer.Enabled = false;

            //Debug
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                exceptions.Add(ex);
            }

            return new Position(position.Latitude, position.Longitude);
        }

        public static void OutputException(Exception e)
        {
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.StackTrace);
            exceptions.Add(e);
        }
    }
}