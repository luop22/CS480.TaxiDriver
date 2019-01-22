using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace KCDriver
{
    public partial class App : Application
    {
        static bool dark = false;
        public App()
        {
            InitializeComponent();
            
            MainPage = new NavigationPage(new WelcomePage())
            {
                BarTextColor = Color.Yellow
            };
            
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
