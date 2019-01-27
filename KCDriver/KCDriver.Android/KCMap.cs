using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps.Model;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms.Maps.Android;
using Android.Content;
using KCDriver.Droid;

namespace KCDriver.Droid
{
    // This class is based off information here: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/map/polyline-map-overlay
    public class KCMap : Map
    {
        public KCMap()
        {
            KCApi.Properties.Map = this;
            KCApi.Properties.MapReady = true;
        }
    }
}