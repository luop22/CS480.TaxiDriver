using System;
using Xamarin.Forms;

namespace KCDriver.Droid
{
    public class Ride
    {
        public int RideId { get; private set; }
        public String ClientName { get; private set; }
        public double ClientLat { get; private set; }
        public double ClientLong { get; private set; }
        public String PhoneNum { get; private set; }
        public string DisplayAddress { get; private set; }

        public Ride() { }

        public Ride(Ride other)
        {
            RideId = other.RideId;
            ClientName = other.ClientName;
            ClientLat = other.ClientLat;
            ClientLong = other.ClientLong;
            PhoneNum = other.PhoneNum;
            DisplayAddress = other.DisplayAddress;
        }

        public Ride(int RideId, String ClientName, double ClientLat, double ClientLong, /*DateTime StartTime,*/ String PhoneNum) {
            this.RideId = RideId;
            this.ClientName = ClientName;
            this.ClientLat = ClientLat;
            this.ClientLong = ClientLong;
            this.PhoneNum = PhoneNum;
            DisplayAddress = "Retrieving address...";
        }

        public void CallClient() {
            Device.OpenUri(new Uri("tel:" + PhoneNum));
        }

        public void SetRidePhoneNum(string num)
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

        public void SetName(string newName)
        {
            ClientName = newName;
        }

        public void SetDisplayAddress(string address)
        {
            DisplayAddress = address;
        }
    }
}
