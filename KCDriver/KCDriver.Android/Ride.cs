using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace KCDriver
{
    class Ride
    {
        private int rideId = 0;
        private int numberOfRiders = 1;
        private String clientName = "";
        private double clientLocation = 0.0;
        private DateTime startTime;
        private DateTime endTime;
        private String phoneNum;
        

        Ride(int rID, int numR, String cname, double loc, DateTime sTime) {
            rideId = rID;
            numberOfRiders = numR;
            clientName = cname;
            clientLocation = loc;
            startTime = sTime;
        }

        public int getRideId() {
            return rideId;
        }

        public void setRideID(int rId) {
            rideId = rId;
        }

        public int getNumberRiders() {
            return numberOfRiders;
        }

        public void setNumberRiders(int numRiders) {
            numberOfRiders = numRiders;
        }

        public string getClientName() {
            return clientName;
        }

        public void setClientName(string cname) {
            clientName = cname;
        }

        public double getClientLocation() {
            return clientLocation;
        }

        public void setClientLocation(double loc) {
            clientLocation = loc;
        }

        public DateTime getStartTime() {
            return startTime;
        }

        public void setStartTime(DateTime sTime) {
            startTime = sTime;
        }

        public DateTime getEndTime() {
            return endTime;
        }

        public void setEndTime(DateTime eTime) {
            endTime = eTime;
        }

        public void callClient() {
            Device.OpenUri(new Uri("tel:" + phoneNum));
        }

    }
}
