using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Android.Widget;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.MediaManager;
using Plugin.MediaManager.Abstractions.Implementations;
using Plugin.MediaManager.Abstractions.Enums;
using System.IO;
using Android.Content.Res;
using Plugin.MediaManager.Abstractions;
using Android.Media;

namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        object buttonLock;

        /// <summary>
        /// Asks for permission to access location and initializes the sign in page.
        /// </summary>
        public SignInPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            KCApi.Properties.AskingLocationPermission = false;
            KCApi.Properties.HaveLocationPermission = false;

            buttonLock = new object();
        }

        /// <summary>
        /// makes sure that the app has permission to access the 
        /// location and resets KCApi everytime the sign in page appears.
        /// </summary>
        protected override async void OnAppearing()
        {
            if (KCApi.Properties.State != KCProperties.AppState.SignIn)
            {
                KCApi.Properties.State = KCProperties.AppState.SignIn;

                KCApi.Reset();

                KCApi.Properties.HaveLocationPermission = await RequestPermission(Plugin.Permissions.Abstractions.Permission.Location);
            }
        }

        /// <summary>
        /// Attempts to sign the user in. If location permissions are 
        /// denied and denied again, the app is killed.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void SignInClicked(object sender, EventArgs e)
        {
            lock (buttonLock)
            {
                if (KCApi.Properties.State == KCProperties.AppState.SignIn)
                {
                    //only allow the user to get to the next page if the username and password are correct.
                    if (KCApi.Properties.HaveLocationPermission)
                    {
                        if ((!String.IsNullOrEmpty(username.Text) || !String.IsNullOrEmpty(password.Text)) && KCApi.Authenticate(password.Text, username.Text))
                        {
                            KCApi.Properties.State = KCProperties.AppState.Transitioning;
                            KCApi.Properties.NetState = KCProperties.NetworkState.Connected;

                            //reset the username and password fields.
                            username.Text = "";
                            password.Text = "";
                            Navigation.PushAsync(new AcceptPage());
                        }
                        else
                        {
                            //The username or password is incorrect.
                            var text = "Check your network connection and credentials and try again.";
                            DisplayAlert("Authentication Failure", text, "OK");
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
            }
        }

        /// <summary>
        /// Function to request a particular permission enumerated by the Permissions Plugin.
        /// </summary>
        /// <param name="permission">The permission to be requested.</param>
        /// <returns>A task containing either true if the permission is granted or false otherwise.</returns>
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

        /// <summary>
        /// Opens the phone app to call the K.C. Cab company.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void CallSelect(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("tel:" + "5099293055"));
        }
    }
}