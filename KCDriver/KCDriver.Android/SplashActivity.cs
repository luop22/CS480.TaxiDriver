
using Android.App;
using Android.Content;
using Android.OS;


using Xamarin.Forms;

//Splash screen. Currently broken.

namespace KCDriver.Droid
{
    [Activity(Theme = "@style/KCTheme.Splash", NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(new Intent(Android.App.Application.Context, type: typeof(MainActivity)));
        }

        public override void OnBackPressed() { }
    }
}