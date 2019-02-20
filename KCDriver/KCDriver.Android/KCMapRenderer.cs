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

// Source: https://docs.microsoft.com/en-us/xamarin/android/platform/maps-and-location/maps/maps-api
[assembly: ExportRenderer(typeof(KCMap), typeof(KCMapRenderer))]
namespace KCDriver.Droid
{
    public class KCMapRenderer : MapRenderer
    {
        private readonly object dataLock;
        private readonly object nativeMapLock = new object();
        Android.Gms.Maps.Model.Polyline CurrentLine;
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
                /*case "RouteCoordinates":
                    DrawPolylineFromRouteCoordinates();
                    break;*/

                /*case "CurrentPosition":
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Position temp = KCApi.Properties.CurrentPosition;
                        AnimateCameraTo(temp.Latitude, temp.Longitude);
                    });
                    break;*/

                /*case "InterpolatedPosition":
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Position temp = KCApi.Properties.InterpolatedPosition;
                        AnimateCameraTo(temp.Latitude, temp.Longitude);
                    });
                    break;*/
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

            KCApi.Properties.RenderReady = true;
            KCApi.Properties.Renderer = this;

            mapDrawn = true;
        }

        // The number 18.5f reflects some adjustments I made and tested.
        // The max appears to be 20.f for whatever reason
        public void AnimateCameraTo(double lat, double lon)
        {
            try
            {
                if (KCApi.Properties.MapReady && KCApi.Properties.RenderReady)
                {
                    lock (nativeMapLock)
                    {
                        NativeMap.MoveCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lon)));
                        NativeMap.MoveCamera(CameraUpdateFactory.ZoomTo(18.5f));
                    }
                }
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
            }
        }

        // In order to change the UI, you must invoke from the main (UI) thread!
        public void DrawPolylineFromRouteCoordinates()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (CurrentLine != null)
                    CurrentLine.Remove();

                var polylineOptions = new PolylineOptions();
                polylineOptions.InvokeColor(0x66FF0000);

                foreach (var position in KCApi.Properties.RouteCoordinates)
                {
                    polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
                }

                CurrentLine = NativeMap.AddPolyline(polylineOptions);
            });
        }
    }
}