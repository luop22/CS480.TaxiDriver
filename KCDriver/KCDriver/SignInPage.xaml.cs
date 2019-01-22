using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KCDriver
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SignInPage : ContentPage
	{
        public SignInPage ()
		{
			InitializeComponent ();
        }
        private void SignInClicked(object sender, EventArgs e)
        {
            //only allow the user to get to the next page if the username and password are corrent.
           // if (Authenticate()) {
                Navigation.PushAsync(new AcceptPage());
          //  }
        }

        //this function will send the usename and password to the server to be authenticated.
        /*
        private bool Authenticate() {
            string userName = usernameEntry.Text;
            string password = passwordEntry.Text;
            
            return true;
        }
        */

    }
}