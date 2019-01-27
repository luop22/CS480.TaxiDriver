// Note: distribution requires MIT license.

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