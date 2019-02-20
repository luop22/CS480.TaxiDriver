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
        MapPage mapPage;

        public AcceptPage(Driver_Id driver)
        {
            this.driver = driver;
            InitializeComponent();
            mapPage = new MapPage();
        }

        void Button_Clicked(object sender, EventArgs e)
        {

            //call to 
            //if (there is a ride ) {
                //Start takes only a position, which will come from the database
                KCApi.Start(Test.a);
                Navigation.PushAsync(mapPage);
            //}
        }
    }
}