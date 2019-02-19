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

            if ((!String.IsNullOrEmpty(username.Text) || !String.IsNullOrEmpty(password.Text)) && request.Authenticate(password.Text, username.Text)) {
                Navigation.PushAsync(new AcceptPage());
            } else {
                //The username and password is incorrect.
                //add a message on the sign in page giving an error.
            }

        }
    }
}