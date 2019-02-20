using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace KCDriver.Droid {
    public class Driver_Id {

        public int driver_Id { get; set; }
        public String token { get; set; }

        public Driver_Id(int driver_Id,String token) {

            this.driver_Id = driver_Id;
            this.token = token;

        }

    }
}