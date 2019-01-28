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

using KCDriver.Droid;

namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KCApp : Xamarin.Forms.Application
    {
        public static double ScreenHeight;
        public static double ScreenWidth;

        public KCApp()
        {
            if (Task.Run(async () => await this.GetLocationPermissionAsync()).Result)
            {
                var navPage = new NavigationPage(new WelcomePage())
                {
                    BarTextColor = Color.Yellow
                };
                MainPage = navPage;
            }
            else return; // Display notice that location is needed. Disable navigation entirely? Camera still needs to be interpolated either way.
        }

        async Task<bool> GetLocationPermissionAsync()
        {
            // Code modeled after Permission Plugin example: https://github.com/jamesmontemagno/PermissionsPlugin
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Location))
                    {
                        await MainPage.DisplayAlert("Location required.", "Location required for navigation animation.", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Location);

                    if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Location))
                        status = results[Plugin.Permissions.Abstractions.Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
                    return true;
                }
                else
                {
                    await MainPage.DisplayAlert("Permission Denied", "Location permission is required. Please restart.", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}