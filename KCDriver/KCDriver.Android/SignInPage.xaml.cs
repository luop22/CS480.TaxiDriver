using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KCDriver.Droid
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SignInPage : ContentPage
	{
        public SignInPage ()
		{
			InitializeComponent ();

            NavigationPage.SetHasNavigationBar(this, true);
        }

        private void SignInClicked(object sender, EventArgs e)
        {
            
            ServerRequests request = new ServerRequests();

            Driver_Id driver = request.Authenticate(password.Text, username.Text);

            if ((!String.IsNullOrEmpty(username.Text) || !String.IsNullOrEmpty(password.Text)) && driver != null) {
                Navigation.PushAsync(new AcceptPage(driver));
            } else {
                //The username and password is incorrect.
                //add a message on the sign in page giving an error.
            }

        }
        private void callSelect(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("tel:" + "5099293055"));
        }
    }
}