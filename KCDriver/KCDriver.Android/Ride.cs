using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace KCDriver.Droid
{
    public class Ride
    {
        public int RideId { get; private set; }
        public int NumberOfRiders { get; private set; }
        public String ClientName { get; private set; }
        public double ClientLat { get; private set; }
        public double ClientLong { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public String PhoneNum { get; private set; }

        public Ride() { }

        public Ride(int RideId, int NumberOfRiders, String ClientName, double ClientLat, double ClientLong, DateTime StartTime, String PhoneNum) {
            this.RideId = RideId;
            this.NumberOfRiders = NumberOfRiders;
            this.ClientName = ClientName;
            this.ClientLat = ClientLat;
            this.ClientLong = ClientLong;
            this.StartTime = StartTime;
            this.PhoneNum = PhoneNum;
        }

        public void CallClient() {
            Device.OpenUri(new Uri("tel:" + PhoneNum));
        }

        public void SetRidePhoneNum(String num)
        {
            PhoneNum = num;
        }

        public void SetRideID(int newID)
        {
            RideId = newID;
        }

        public void SetPosition(double newLat, double newLong)
        {
            ClientLat = newLat;
            ClientLong = newLong;
        }
    }
}
