using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

namespace KCDriver.Droid
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
		public MapPage ()
		{
			InitializeComponent();
            var map = new KCMap()
            {
                MapType = MapType.Street,
                WidthRequest = KCApp.ScreenWidth,
                HeightRequest = KCApp.ScreenHeight,
                IsShowingUser = true
            };

            Content = map;
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }
    }
}