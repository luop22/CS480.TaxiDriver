using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Android.Widget;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;


namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        public SignInPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, true);

            KCApi.Properties.AskingLocationPermission = false;
            KCApi.Properties.HaveLocationPermission = false;
        }

        protected override async void OnAppearing()
        {
            if (KCApi.Properties.AskingLocationPermission)
                return;

            KCApi.Properties.AskingLocationPermission = true;
            KCApi.Properties.HaveLocationPermission = await RequestPermission(Plugin.Permissions.Abstractions.Permission.Location);
            KCApi.Properties.AskingLocationPermission = false;
        }

        private void SignInClicked(object sender, EventArgs e)
        {
            //only allow the user to get to the next page if the username and password are correct.
            if (KCApi.Properties.HaveLocationPermission)
            {
                if ((!String.IsNullOrEmpty(username.Text) || !String.IsNullOrEmpty(password.Text)) && KCApi.Authenticate(password.Text, username.Text))
                {
                    //reset the username and password fields.
                    username.Text = "";
                    password.Text = "";
                    Navigation.PushAsync(new AcceptPage());
                }
                else
                {
                    //The username and password is incorrect.
                    var text = "Incorrect credentials.";
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                }
            }
            else
            {
                var alertDialog = new Android.App.AlertDialog.Builder(CrossCurrentActivity.Current.Activity);
                alertDialog.SetTitle("Location Needed");
                alertDialog.SetMessage("You must allow this app to use your location. Allow and try again.");
                alertDialog.SetPositiveButton("OK", async (senderad, args) =>
                {
                    KCApi.Properties.HaveLocationPermission = await RequestPermission(Plugin.Permissions.Abstractions.Permission.Location);
                });
                alertDialog.SetNegativeButton("Cancel", (senderad, args) =>
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                });
                alertDialog.Create().Show();
            }
        }

        //this function will send the usename and password to the server to be authenticated.
        /*
        private bool Authenticate() {
            string userName = usernameEntry.Text;
            string password = passwordEntry.Text;
            
            return true;
        }
        */

        public async Task<bool> RequestPermission(Plugin.Permissions.Abstractions.Permission permission)
        {
            try
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(permission);
                var status = PermissionStatus.Unknown;
                //Best practice to always check that the key exists
                if (results.ContainsKey(permission))
                    status = results[permission];

                return status == PermissionStatus.Granted;
            }
            catch
            {

                var text = "Error requesting permissions";
                Toast.MakeText(CrossCurrentActivity.Current.Activity, text, ToastLength.Short).Show();
                return false;
            }
        }

        private void callSelect(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("tel:" + "5099293055"));
        }
    }
}