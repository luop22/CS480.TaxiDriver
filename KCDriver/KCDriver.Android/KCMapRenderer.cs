using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Xamarin.Forms;

using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Maps;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using System.Threading.Tasks;
using Android.Locations;
using System.ComponentModel;
using KCDriver.Droid;
using Android.Views;

// Source: https://docs.microsoft.com/en-us/xamarin/android/platform/maps-and-location/maps/maps-api
[assembly: ExportRenderer(typeof(KCMap), typeof(KCMapRenderer))]
namespace KCDriver.Droid
{
    public class KCPin : Pin
    {
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

        public MarkerOptions CreateMarker()
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(this.Position.Latitude, this.Position.Longitude));
            marker.SetTitle(this.Label);
            marker.SetSnippet(this.Address);
            marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.taxiIcon));
            return marker;
        }
    }

    public partial class KCMapRenderer : MapRenderer
    {
        private readonly object dataLock;
        private readonly object nativeMapLock = new object();
        KCPin riderPin;
        bool mapDrawn = false;

        public KCMapRenderer(Context context) : base(context)
        {
            dataLock = new object();
            // Add the property changed event to our event handler in KCMapRenderer
            KCApi.Properties.PropertyChanged += new PropertyChangedEventHandler(OnElementPropertyChanged);

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "CurrentRide":
                    UpdateMarker();
                    break;
            }
        }

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

        // The number 18.5f reflects some adjustments I made and tested.
        // The max appears to be 20.f for whatever reason
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

        public void UpdateMarker()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (NativeMap == null);

                NativeMap.Clear();
                riderPin = KCPin.CreateRiderPin(new Position(KCApi.Properties.CurrentRide.ClientLat,
                                                            KCApi.Properties.CurrentRide.ClientLong));
                MarkerOptions mo = riderPin.CreateMarker();
                NativeMap.AddMarker(mo);
            });
        }
    }
}