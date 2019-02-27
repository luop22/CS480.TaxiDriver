﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

//class which handles calls to the server including authentication and ride requestes
namespace KCDriver.Droid {

    static partial class KCApi {

        //authentication functions
        //returns true if the user is authenticated with the server
        public static bool Authenticate(String password, String userName) {

            String message = "http://148.72.40.62/driver/auth/authenticate.php?username=" + userName + "&pwHsh=" + GetHash(password, userName);
            string responseFromServer = "";
            try {
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            responseFromServer = reader.ReadToEnd();
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
            } catch (Exception e) {
                return false;
            }

            if (!responseFromServer.Contains("error")) {
                String[] data = responseFromServer.Split(new char[] { '"', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

                Driver_Id.driver_Id = Int32.Parse(data[4]);
                Driver_Id.token = data[6];
                Driver_Id.authenticated = true;
                return true;
            }

            return false;
        }

        //calculates the hash of the password with the salt
        private static string GetHash(string input, String userName) {
            using (SHA256 sha256Hash = SHA256.Create()) {
                HashAlgorithm hashAlgorithm = sha256Hash;
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(GetSalt(userName) + input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++) {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        //gets the salt of a user
        public static String GetSalt(String userName) {
            String message = "http://148.72.40.62/driver/auth/getSalt.php?username=" + userName;
            string responseFromServer = "";
            try {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Display the status.
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Display the content.
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return "Error Connecting to Server";
            }

            if (!responseFromServer.Contains("error")) {
                String[] data = responseFromServer.Split(new char[] { '"', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
                return data[4];
            } else {
                return "error";
            }
        }

        public static String CheckQueue()
        {
            String message = "http://148.72.40.62/driver/checkQueue.php";
            string responseFromServer = "";
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            try {
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Display the status.
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Display the content.
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return "Error Connecting to Server";
            }
            //If

            if (responseFromServer.Contains("Rides are available")) {
                return "Rides are available";
            } else if (responseFromServer.Contains("No available rides")) {
                return "No available rides";
            } else if ((responseFromServer.Contains("error"))) {
                return "error";
            } else {
                return "error";
            }
        }

        public static bool AcceptNextRide(Ride ride)
        {
            string message = "http://148.72.40.62/driver/acceptRide.php?token=" + Driver_Id.token + "&driverID=" + Driver_Id.driver_Id;
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            // Get the response.
            string responseFromServer = "";
            try {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return false;
            }
            
            try {
                String[] data = responseFromServer.Split(new char[] { '"', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (!responseFromServer.Contains("error"))
                {
                    ride.SetRideID(Int32.Parse(data[2]));
                }
                //If the response comes back as Authentication failure then set the driver as not authenticated.
                else if (responseFromServer.Contains("Authentication failure")) {
                    Driver_Id.authenticated = false;
                    return false;
                }
                else
                {
                    ride.SetRideID(Int32.Parse(data[6]));
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool SetRideLocation(Ride ride, double latitude, double longitude)
        {
            string message = "http://148.72.40.62/driver/rideStatus.php?driverID=" + Driver_Id.driver_Id
                + "&token=" + Driver_Id.token + "&rideID=" + ride.RideId + "&lat=" + latitude + "&lon=" + longitude;
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            // Get the response.
            string responseFromServer = "";
            try { 
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return false;
            }
            //If the response comes back as Authentication failure then set the driver as not authenticated.
            if (responseFromServer.Contains("Authentication failure") || responseFromServer.Contains("Unable to authenticate")) {
                Driver_Id.authenticated = false;
                return false;
            }
            //check if there are any other errors
            else if (!responseFromServer.Contains("error"))
            {
                try
                {
                    //If there are no errors add the lat and long to the ride class and return true.
                    String[] data = responseFromServer.Split(new char[] { '"', ',', ':', '}' }, StringSplitOptions.RemoveEmptyEntries);

                    double rideLat = Double.Parse(data[4]);
                    double rideLong = Double.Parse(data[6]);

                    ride.SetPosition(rideLat, rideLong);
                    return true;
                }
                catch (Exception e)
                {
                    KCApi.OutputException(e);
                    return false;
                }
            }

            return true;
        }

        public static bool CompleteRide(Ride ride)
        {
            string message = "http://148.72.40.62/driver/completeRide.php?token=" + Driver_Id.token + "&driverID=" 
                + Driver_Id.driver_Id + "&rideID=" + ride.RideId;

            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            if (!responseFromServer.Contains("error"))
            {
                return true;
            }

            return false;
        }

        public static bool SetDriverLocation(double latitude, double longitude)
        {
            string message = "http://148.72.40.62/driver/updateLocation.php?driverID=" +  Driver_Id.driver_Id
                + "&token=" + Driver_Id.token + "&lat=" + latitude + "&lon=" + longitude;
            string responseFromServer = "";
            try {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return false;
            }
            //If the response comes back as Authentication failure then set the driver as not authenticated.
            if (responseFromServer.Contains("Authentication failure") || responseFromServer.Contains("Unable to authenticate")) {
                Driver_Id.authenticated = false;
                return false;
            }
            if (!responseFromServer.Contains("error"))
            {
                return true;
            }

            return false;
        }

        public static bool CancelRide(Ride ride)
        {
            string message = "http://148.72.40.62/driver/decouple.php?" + "rideID=" + ride.RideId + "&token=" + Driver_Id.token +
                 "&driverID=" + Driver_Id.driver_Id;
            string responseFromServer = "";
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            try {
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (Exception e) {
                return false;
            }


            if (!responseFromServer.Contains("error"))
            {
                return true;
            }

            return false;
        }
    }
}
