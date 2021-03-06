﻿using System;
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
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Android.Widget;
using Plugin.CurrentActivity;

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
            
            Properties.RideStatus = KCProperties.RideStatuses.Uninitialized;

            // The timer automatically updates the camera and position every interval.
            updatePositionTimer = new System.Timers.Timer(500.0f);
            updatePositionTimer.Elapsed += (o,e) => Task.Factory.StartNew( () => UpdatePosition(o,e));

            updateCameraTimer = new System.Timers.Timer(16.66f);
            updateCameraTimer.Elapsed += (o, e) => Task.Factory.StartNew(() => UpdateCamera(o, e));
        }

        /// <summary>
        /// A timer calls this 10 times per second. Updates the db on where the driver is
        /// and updates the driver (device) on where the rider is.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="e">Event arguments.</param>
        public static void UpdatePosition(Object source, ElapsedEventArgs e)
        {
            updatePositionTimer.Interval = 500.0f;

            if (KCApi.Properties.State != KCProperties.AppState.Map)
                return;

            if (!Properties.MapReady || !Properties.RenderReady)
                return;

            Task.Run( async () => {
                try
                {
                    Position tempPos = await GetCurrentPosition(20);

                    if (tempPos.Latitude != Properties.CurrentPosition.Latitude
                        || tempPos.Longitude != Properties.CurrentPosition.Longitude)
                        Properties.CurrentPosition = tempPos;

                    Ride temp = new Ride(Properties.CurrentRide);

                    SetRideLocation(temp, Properties.CurrentPosition.Latitude, Properties.CurrentPosition.Longitude);

                    if (KCApi.Properties.RideStatus == KCProperties.RideStatuses.Active)
                    {
                        // Only update current ride if needed, since it will trigger a UI update.
                        if (Properties.CurrentRide == null || temp.ClientLat != Properties.CurrentRide.ClientLat
                        || temp.ClientLong != Properties.CurrentRide.ClientLong)
                            Properties.CurrentRide = temp;
                    }
                }
                catch (Exception ex)
                {
                    OutputException(ex);
                }
            }); 
        }

        /// <summary>
        /// Function to keep camera locked on correct location on them map or
        /// allow the user to scroll.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="e">Event arguments.</param>
        public static void UpdateCamera(Object source, ElapsedEventArgs e)
        {
            updateCameraTimer.Interval = 16.66f;

            if (KCApi.Properties.State != KCProperties.AppState.Map)
                return;

            if (!Properties.MapReady || !Properties.RenderReady)
                return;

            //Keeps the camera locked on where it is supposed to be.
            Device.BeginInvokeOnMainThread( () =>
            {
                Position temp = new Position();

                if (Properties.CameraOnDriver)
                    temp = Properties.CurrentPosition;
                else if (Properties.CameraOnRider)
                    temp = new Position(Properties.CurrentRide.ClientLat, Properties.CurrentRide.ClientLong);

                if (Properties.CameraOnDriver || Properties.CameraOnRider)
                {
                    if (temp != null)
                        Properties.Renderer.MoveCameraTo(temp.Latitude, temp.Longitude);

                    Properties.Map.HasScrollEnabled = false;
                }
                else Properties.Map.HasScrollEnabled = true;
            });
        }

        /// <summary>
        /// Starts navigation functions. Takes riders position and destination address string.
        /// </summary>
        /// <param name="ride">Ride object wtih active ride information.</param>
        public static void Start(Ride ride)
        {
            Properties.CurrentRide = ride;
            updatePositionTimer.Start();
            updateCameraTimer.Start();

            Properties.RideStatus = KCProperties.RideStatuses.Active;
        }

        /// <summary>
        /// Stops navigation, does cleanup, and outputs recorded exceptions in debug.
        /// </summary>
        public static void Stop()
        {
            updatePositionTimer.Stop();
            updateCameraTimer.Stop();

            Properties.RideStatus = KCProperties.RideStatuses.Uninitialized;

            //Debug
            #if DEBUG
            Task.Run(() => { 
                lock (exceptionsLock)
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

                    Debug.Flush();
                    Debug.WriteLine("------------------------- End -------------------------------------");
                }
            });
            #endif
        }

        public static void Reset()
        {
            Properties.CurrentRide = null;
            Properties.NetState = KCProperties.NetworkState.Connected;
        }

        /// <summary>
        /// Function to get the current position. Returns 0,0 on failure.
        /// </summary>
        /// <param name="timeoutSeconds">Timeout for the GPS call. Default 20 seconds.</param>
        /// <returns>Current position of the device.</returns>
        public static async Task<Position> GetCurrentPosition(int timeoutSeconds = 20)
        {

            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;

                Plugin.Geolocator.Abstractions.Position position = await locator.GetLastKnownLocationAsync();

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

                position = await locator.GetPositionAsync(TimeSpan.FromSeconds(timeoutSeconds), null, true);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Geolocation not"))
                    OutputException(e);
            }

            return new Position(0, 0);
        }

        /// <summary>
        /// Takes a geographical location and returns an address using
        /// Google's Geocoding API.
        /// </summary>
        /// <param name="pos">Position from which to get an address.</param>
        /// <returns>A string containing the address or an error message.</returns>
        public static string GetAddressFromPosition(Position pos)
        {
            try
            {
                string request = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + pos.Latitude + "," + pos.Longitude
                    + "&key=AIzaSyDku7O-5lR0g8fOx04lPSAPA5T8-JmK1a0";

                WebClient client = new WebClient();
                string s = client.DownloadString(request);

                var obj = JsonConvert.DeserializeObject<GoogleRevGeocoderResponse>(s);

                if (obj.results.Count > 0)
                    return obj.results[0].formatted_address;
                else return "No nearby address. Retrying...";
            }
            catch (Exception e)
            {
                OutputException(e);
                return "No nearby address. Retrying...";
            }
            
        }

        /// <summary>
        /// Writes the exception to the Debug log and saves it to output 
        /// again later.
        /// </summary>
        /// <param name="e">Exception to output.</param>
        public static void OutputException(Exception e)
        {
            lock(exceptionsLock)
            {
                exceptions.Add(e);
            }
        }

        /// <summary>
        /// Checks whether the current user has given location permissions.
        /// </summary>
        /// <returns>True if permissions have been granted, false otherwise.</returns>
        public static bool CheckLocationPermission()
        {
            try
            {
                PermissionStatus ps = Task.Run(async () => await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location)).Result;

                return ps == PermissionStatus.Granted;
            }
            catch (Exception e)
            {
                OutputException(e);
                return false;
            }
        }
    }
}