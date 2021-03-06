﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps.Model;
using Android.Gms.Common;
using Android.Gms.Location;


using Xamarin.Forms.Maps.Android;
using Android.Content;

using KCDriver.Droid;

namespace KCDriver.Droid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KCApp : Xamarin.Forms.Application
    {
        public static double ScreenHeight;
        public static double ScreenWidth;

        /// <summary>
        /// initilises the KCApp class.
        /// </summary>
        public KCApp()
        {
            MainPage = new NavigationPage(new SignInPage());
        }

        protected override void OnStart()
        {

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