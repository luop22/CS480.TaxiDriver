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
	public partial class WelcomePage : ContentPage
	{

        public WelcomePage()
        {
            InitializeComponent ();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(3000);
            Application.Current.MainPage = new NavigationPage(new SignInPage());
            await this.Navigation.PushAsync(new SignInPage());
        }
    }
}