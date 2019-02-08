// Distribution requires MIT license.

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
using Android.Gms.Common;
using Android.Gms.Location;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms.Maps.Android;
using Android.Content;

namespace KCDriver.Droid
{
    // Code here taken from Permission Plugin intstructions
    [Activity(Label = "K.C Driver App", Icon = "@drawable/taxiIcon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);

            var width = Resources.DisplayMetrics.WidthPixels;
            var height = Resources.DisplayMetrics.HeightPixels;
            var density = Resources.DisplayMetrics.Density;

            KCApp.ScreenWidth = (width - 0.5f) / density;
            KCApp.ScreenHeight = (height - 0.5f) / density;

            KCApi.Initialize();

            LoadApplication(new KCApp());
        }

        // Permissions function required by Permissions Plugin
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}