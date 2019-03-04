using Xamarin.Forms.Maps;

namespace KCDriver.Droid {
    // This class is based off information here: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/map/polyline-map-overlay
    public class KCMap : Map
    {
        public KCMap()
        {
            KCApi.Properties.Map = this;
            KCApi.Properties.MapReady = true;
        }
    }
}