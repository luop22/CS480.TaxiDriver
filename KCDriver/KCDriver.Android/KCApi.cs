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
    public class Test
    {
        public static Position a = new Position(47.9220033, -122.22861);
        public static Position b = new Position(47.0143142, -120.5318273);
        public static Position c = new Position(47.6548883, -122.3095367);

        // Round trip from some bus stop in Everett to all over
        public static string request = "https://maps.googleapis.com/maps/api/directions/json?" +
            "origin=" + a.Latitude + "," + a.Longitude + "&destination=" + a.Latitude + "," + a.Longitude +
            "&waypoints=" + b.Latitude + "," + b.Longitude + "|" + c.Latitude + "," + c.Longitude +
            "&key=AIzaSyAgdPpZhmK2UGsVKkJ5UWGp-w46aSt2Npo";
    }

    /*
     * Note: 
    */
    static partial class KCApi
    {
        public static KCProperties Properties = new KCProperties();
        private static System.Timers.Timer updatePositionTimer;
        private static System.Timers.Timer interpolateTimer;
        private static List<Exception> exceptions = new List<Exception>();

        // Set default values and start timers
        public static void Initialize()
        {
            Properties.MapReady = false;
            Properties.RenderReady = false;
            Properties.RouteCoordinates = new List<Position>();

            // The timer automatically updates the camera and position every interval.
            updatePositionTimer = new System.Timers.Timer(16.66f);
            updatePositionTimer.Elapsed += (o,e) => Task.Factory.StartNew( () => UpdatePosition(o,e));

            interpolateTimer = new System.Timers.Timer(16.66f);
            //interpolateTimer.Elapsed += Interpolate;
            //interpolateTimer.AutoReset = true;
        }

        // A timer calls this 60 times per second. It checks for updates in position and updates the route list to reflect when 
        // the driver has gone farther than a certain point still in the que.
        public static void UpdatePosition(Object source, ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => {

                updatePositionTimer.Stop();
                Position p = GetCurrentPosition();

                if (Properties.CurrentPosition != p)
                {
                    Properties.CurrentPosition = p;

                    if (Properties.RouteCoordinates.Count > 1)
                    {
                        int closestIndex = 0;
                        Xamarin.Forms.Maps.Distance smallest = DistanceTo(Properties.CurrentPosition, Properties.RouteCoordinates.First());
                        for (int i = 0; i < Properties.RouteCoordinates.Count; ++i)
                        {
                            if (DistanceTo(Properties.CurrentPosition, Properties.RouteCoordinates.ElementAt(i)).Miles < smallest.Miles)
                            {
                                smallest = DistanceTo(Properties.CurrentPosition, Properties.RouteCoordinates.ElementAt(i));
                                closestIndex = i;
                            }
                        }

                        if (closestIndex > 0)
                        {
                            List<Position> temp = new List<Position>();
                            temp.AddRange(Properties.RouteCoordinates);
                            temp.RemoveRange(0, closestIndex + 1);
                            Properties.RouteCoordinates = temp;
                        }
                    }
                }

                //Keeps the camera locked on the user
                /*Device.BeginInvokeOnMainThread(() =>
                {
                    Position temp = KCApi.Properties.CurrentPosition;
                    KCApi.Properties.Renderer.AnimateCameraTo(temp.Latitude, temp.Longitude);
                });*/

                updatePositionTimer.Interval = 16.66f;
                updatePositionTimer.Start();
            });
        }

        // Starts navigation functions. Takes riders position and destination address string.
        public static void Start(Ride ride, string destination = "")
        {
            ThreadPool.QueueUserWorkItem(o => {
                //Wait until the map and renderer are initialized.
                while (!Properties.MapReady || !Properties.RenderReady) { }

                Properties.CurrentRide = ride;
                //Properties.Renderer.UpdateMarker();

                List<Position> temp = new List<Position>();
                while (temp.Count == 0)
                    temp = GetPolyline(KCApi.GenerateRequest(new Position(Properties.CurrentRide.ClientLat, Properties.CurrentRide.ClientLong), destination));

                Properties.RouteCoordinates = temp;
            });

            updatePositionTimer.Enabled = true;
            interpolateTimer.Enabled = true;
        }

        // Stops navigation, does cleanup, and outputs recorded exceptions in debug.
        public static void Stop()
        {
            updatePositionTimer.Enabled = false;
            interpolateTimer.Enabled = false;

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

        // Creates the HTTP request to be sent to Google using the start lat and long and the destination address.
        public static string GenerateRequest(Position pointA, string pointB = "")
        {
            string s = "";

            try
            {
                Xamarin.Forms.Maps.Geocoder g = new Xamarin.Forms.Maps.Geocoder();
                var origin = GetCurrentPosition();
                List<Position> pointBPossible = null;
                Position pointBConverted;

                if (pointB != "")
                {
                    pointBPossible = Task.Run(async () => await g.GetPositionsForAddressAsync(pointB)).Result.ToList<Position>();
                }
                

                if (pointBPossible != null && pointBPossible.Count > 0)
                {
                    pointBConverted = pointBPossible.ElementAt<Position>(0);
                    return "https://maps.googleapis.com/maps/api/directions/json?" +
                    "origin=" + origin.Latitude + "," + origin.Longitude + "&destination=" + pointBConverted.Latitude + "," + pointBConverted.Longitude +
                    "&waypoints=" + pointA.Latitude + "," + pointA.Longitude +
                    "&key=AIzaSyAgdPpZhmK2UGsVKkJ5UWGp-w46aSt2Npo";
                }
                else
                {
                    // Navigate without the destination, just the person
                    return "https://maps.googleapis.com/maps/api/directions/json?" +
                    "origin=" + origin.Latitude + "," + origin.Longitude + "&destination=" + pointA.Latitude + "," + pointA.Longitude +
                    "&key=AIzaSyAgdPpZhmK2UGsVKkJ5UWGp-w46aSt2Npo";
                }
            }
            catch (Exception e)
            {
                OutputException(e);
            }

            return s;
        }

        // This code is modeled off of a post by gtleal here: https://forums.xamarin.com/discussion/85684/how-can-i-draw-polyline-for-an-encoded-points-string
        // and based on information here: https://developers.google.com/maps/documentation/utilities/polylinealgorithm
        // Turns a Google Maps encoded polyline into a list of positions.
        private static List<Position> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints))
            {
                return null;
            }

            int index = 0;
            var polylineChars = encodedPoints.ToCharArray();
            var poly = new List<Position>();
            int currentLat = 0;
            int currentLng = 0;
            int next5Bits;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                int sum = 0;
                int shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                {
                    break;
                }

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // calculate next longitude
                sum = 0;
                shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                var pos = new Position(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
                poly.Add(pos);
            }

            return poly;
        }

        // Adds every point of the path to the route to be drawn
        public static List<Position> GetPolyline(string request)
        {
            if (request == "") return new List<Position>();

            WebClient client = new WebClient();
            string s = client.DownloadString(request);
            List<Position> path = new List<Position>();

            var obj = JsonConvert.DeserializeObject<GoogleDirectionsResponse>(s);

            foreach (Route route in obj.routes)
            {
                foreach (Leg leg in route.legs)
                {
                    foreach (Step step in leg.steps)
                    {
                        path.AddRange(DecodePolyline(step.polyline.points));
                    }
                }
            }

            return path;
        }

        public static double ToRadians(double input)
        {
            return Math.PI / 180 * input;
        }

        // Math from the internet
        public static Xamarin.Forms.Maps.Distance DistanceTo(this Position self, Position other)
        {
            // Angle in radians along great circle.
            double radians = Math.Acos(
                Math.Sin(ToRadians(self.Latitude)) * Math.Sin(ToRadians(other.Latitude)) +
                Math.Cos(ToRadians(self.Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                    Math.Cos(ToRadians(self.Longitude - other.Longitude))
                );

            // Multiply by the Earth's radius to get the actual distance.
            return Xamarin.Forms.Maps.Distance.FromKilometers(6371 * radians);
        }

        // Work in Progress
        private static void Interpolate(Object source, ElapsedEventArgs e)
        {
            // 1. Determine speed
            // 2. Determine direction
            // 3. Determine time interval
            // 4. i n t e r p o l a t e (change position directly)

            double deltaX = Properties.CurrentPosition.Longitude - Properties.PreviousPosition.Longitude;
            double deltaY = Properties.CurrentPosition.Latitude - Properties.PreviousPosition.Latitude;
            double magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            double totalDistance = Properties.CurrentPosition.DistanceTo(Properties.PreviousPosition).Miles;

            double speed = (totalDistance / Properties.SpeedTime) * 16.66; // Degrees / ms * ms = Degrees

            Properties.InterpolatedPosition = new Position(Properties.CurrentPosition.Longitude + deltaX / magnitude * speed, Properties.CurrentPosition.Latitude + deltaY / magnitude * speed);
        }

        public static void OutputException(Exception e)
        {
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.StackTrace);
            exceptions.Add(e);
        }
    }
}