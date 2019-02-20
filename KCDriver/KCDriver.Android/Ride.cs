using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace KCDriver
{
    public class Ride
    {
        private int RideId { get; set; }
        private int NumberOfRiders { get; set; }
        private String ClientName { get; set; }
        private double ClientLat { get; set; }
        private double ClientLong { get; set; }
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }
        private String PhoneNum { get; set; }

        Ride(int RideId, int NumberOfRiders, String ClientName, double ClientLat, double ClientLong, DateTime StartTime, String PhoneNum) {
            this.RideId = RideId;
            this.NumberOfRiders = NumberOfRiders;
            this.ClientName = ClientName;
            this.ClientLat = ClientLat;
            this.ClientLong = ClientLong;
            this.StartTime = StartTime;
            this.PhoneNum = PhoneNum;
        }

        public void callClient() {
            Device.OpenUri(new Uri("tel:" + PhoneNum));
        }

    }
}
