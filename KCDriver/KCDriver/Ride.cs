using System;
using System.Collections.Generic;
using System.Text;

namespace KCDriver
{
    class Ride
    {
        int rideId = 0;
        int numberOfRiders = 1;
        String clientName = "";
        double clientLocation = 0.0;
        DateTime startTime;
        DateTime endTime;
        
        Ride(int rID, int numR, String cname, double loc, DateTime sTime) {
            rideId = rID;
            numberOfRiders = numR;
            clientName = cname;
            clientLocation = loc;
            startTime = sTime;
        }

        int getRideId() {
            return rideId;
        }

        void setRideID(int rId) {
            rideId = rId;
        }

        int getNumberRiders() {
            return numberOfRiders;
        }

        void setNumberRiders(int numRiders) {
            numberOfRiders = numRiders;
        }

        string getClientName() {
            return clientName;
        }

        void setClientName(string cname) {
            clientName = cname;
        }

        double getClientLocation() {
            return clientLocation;
        }

        void setClientLocation(double loc) {
            clientLocation = loc;
        }

        DateTime getStartTime() {
            return startTime;
        }

        void setStartTime(DateTime sTime) {
            startTime = sTime;
        }

        DateTime getEndTime() {
            return endTime;
        }

        void setEndTime(DateTime eTime) {
            endTime = eTime;
        }

    }
}
