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
    public partial class AcceptPage : ContentPage
    {
        Driver_Id driver;

        public AcceptPage(Driver_Id driver)
        {
            this.driver = driver;
            InitializeComponent();
        }

        void Button_Clicked(object sender, EventArgs e)
        {

            //call to 
            //if (there is a ride ) {
                Navigation.PushAsync(new MapPage());
            //}
        }
    }
}