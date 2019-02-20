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
        MapPage mapPage;

        public AcceptPage()
        {
            InitializeComponent();
            mapPage = new MapPage();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            //GetNextDestinationFromDatabase
            KCApi.Start(Test.a, "326 Alder St, Everett, WA 98204");
            Navigation.PushAsync(mapPage);
        }
    }
}