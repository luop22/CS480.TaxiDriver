
using Android.App;
using Android.Content;
using Android.OS;


using Xamarin.Forms;


namespace KCDriver.Droid
{
    
    [Activity(Label = "Driver App", Icon = "@drawable/taxiIcon", Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //StartActivity(new Intent(Android.App.Application.Context, type: typeof(MainActivity)));
            StartActivity(typeof(MainActivity));
            
        }

        public override void OnBackPressed() { }
    }
    
}

