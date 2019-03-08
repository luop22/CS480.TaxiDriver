using System;
using Android.Content;
using Xamarin.Forms;

using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Maps;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using System.ComponentModel;
using KCDriver.Droid;

// Source: https://docs.microsoft.com/en-us/xamarin/android/platform/maps-and-location/maps/maps-api
[assembly: ExportRenderer(typeof(KCMap), typeof(KCMapRenderer))]
namespace KCDriver.Droid {
    public class KCPin : Pin
    {
        /// <summary>
        /// Function to set up a basic pin for the rider.
        /// </summary>
        /// <param name="pos">Position to create the pin at.</param>
        /// <returns>The pin object created.</returns>
        public static KCPin CreateRiderPin(Position pos)
        {
            var pin = new KCPin
            {
                Type = PinType.Place,
                Position = new Position(pos.Latitude, pos.Longitude),
                Id = "Next Rider"
            };

            return pin;
        }

        /// <summary>
        /// Takes the current pin object and creates a GoogleMap marker
        /// from it.
        /// </summary>
        /// <returns>A MarkerOptions object from which to make a googlemap marker.</returns>
        public MarkerOptions CreateMarker()
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(this.Position.Latitude, this.Position.Longitude));
            marker.SetTitle(this.Label);
            marker.SetSnippet(this.Address);
            marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.user_pin));
            return marker;
        }
    }

    public partial class KCMapRenderer : MapRenderer
    {
        private readonly object dataLock;
        private readonly object nativeMapLock = new object();
        KCPin riderPin;
        bool mapDrawn = false;

        /// <summary>
        /// Initializes the KCMapRenderer properties.
        /// </summary>
        /// <param name="context">Operating context.</param>
        public KCMapRenderer(Context context) : base(context)
        {
            dataLock = new object();
            // Add the property changed event to our event handler in KCMapRenderer
            KCApi.Properties.PropertyChanged += new PropertyChangedEventHandler(OnElementPropertyChanged);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                // The current ride just got updated
                case "CurrentRide":
                    if (KCApi.Properties.RideStatus == KCProperties.RideStatuses.Active)
                        UpdateMarker();
                    break;
            }
        }

        /// <summary>
        /// Updates the map when it is changed.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe
            }

            if (e.NewElement != null)
            {
                var formsMap = (KCMap)e.NewElement;
                Control.GetMapAsync(this);
            }
        }

        /// <summary>
        /// This function sets up basic variables on the GoogleMap as
        /// soon as it is ready.
        /// </summary>
        /// <param name="map">Googlemap object we are using.</param>
        protected override void OnMapReady(Android.Gms.Maps.GoogleMap map)
        {
            if (mapDrawn)
            {
                return;
            }

            base.OnMapReady(map);
            map.UiSettings.ZoomControlsEnabled = false;
            map.UiSettings.MyLocationButtonEnabled = false;
            map.UiSettings.RotateGesturesEnabled = false;
            map.UiSettings.MapToolbarEnabled = false;

            KCApi.Properties.Renderer = this;
            KCApi.Properties.RenderReady = true;

            mapDrawn = true;

            UpdateMarker();
        }

        /// <summary>
        /// Quick function to animate the camera to a location
        /// on the map.
        /// </summary>
        /// <param name="lat">Latitude to move to.</param>
        /// <param name="lon">Longitude to move to.</param>
        /// <param name="zoom">Level to zoom to over 100ms.</param>
        public void AnimateCameraTo(double lat, double lon, float zoom = 0)
        {
            try
            {
                if (zoom == 0)
                    zoom = KCApi.Properties.Renderer.NativeMap.CameraPosition.Zoom;

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (KCApi.Properties.MapReady && KCApi.Properties.RenderReady && NativeMap != null)
                    {
                        lock (nativeMapLock)
                        {
                            NativeMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lon), zoom), 100, null);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
            }
        }

        /// <summary>
        /// Moves the camera directly to the given coordinates.
        /// </summary>
        /// <param name="lat">Latitude to move to.</param>
        /// <param name="lon">Longitude to move to.</param>
        public void MoveCameraTo(double lat, double lon)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (KCApi.Properties.MapReady && KCApi.Properties.RenderReady && NativeMap != null)
                    {
                        lock (nativeMapLock)
                        {
                            NativeMap.MoveCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lon)));
                        }
                    }
                });
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
            }
        }

        /// <summary>
        /// Function to update the GoogleMap with the new 
        /// position of the rider marker.
        /// </summary>
        public void UpdateMarker()
        {
            lock (nativeMapLock)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   try
                   {
                       NativeMap.Clear();
                       riderPin = KCPin.CreateRiderPin(new Position(KCApi.Properties.CurrentRide.ClientLat,
                                                                   KCApi.Properties.CurrentRide.ClientLong));
                       MarkerOptions mo = riderPin.CreateMarker();
                       NativeMap.AddMarker(mo);
                   }
                   catch (Exception e)
                   {
                       KCApi.OutputException(e);
                   }
               });
            }
        }
    }
}