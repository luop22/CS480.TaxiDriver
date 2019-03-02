using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CSharp;

//class which handles calls to the server including authentication and ride requestes
namespace KCDriver.Droid {

    static partial class KCApi {

        private const string ip = "148.72.40.62";

        //authentication functions
        //returns true if the user is authenticated with the server
        public static bool Authenticate(String password, String userName) {

            string message = "http://" + ip + "/driver/auth/authenticate.php?username=" + userName + "&pwHsh=" + GetHash(password, userName);
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
            dynamic jObject = null;
            if (!responseFromServer.Contains("Failure")) {
                jObject = JObject.Parse(responseFromServer);

                Driver_Id.driver_Id = Int32.Parse((string)jObject.result.driverID);
                Driver_Id.token = (string)jObject.result.token;
                Driver_Id.authenticated = true;

                return true;
            } else {
                return false;
                }
            } catch (Exception e) {
                KCApi.OutputException(e);
                return false;
            }
                
        }

        //calculates the hash of the password with the salt.
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

        //gets the salt of a user.
        public static String GetSalt(String userName) {
            String message = "http://" + ip + "/driver/auth/getSalt.php?username=" + userName;
            String responseFromServer = "";
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

                dynamic jObject = null;
                if (!responseFromServer.Contains("Failure")) {
                    jObject = JObject.Parse(responseFromServer);
                    return (string)jObject.result.pwSlt;
                } else {
                    return "error";
                }
            } catch (Exception e) {
                KCApi.OutputException(e);
                return "Error Connecting to Server";
            }
            
        }
        
        //checks if there are any rides in the queue.
        public static String CheckQueue()
        {
            String message = "http://" + ip + "/driver/checkQueue.php";
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

                dynamic jObject = null;
                if (!responseFromServer.Contains("Failure")) {
                    jObject = JObject.Parse(responseFromServer);
                    String result = (string)jObject.result;
                    if (result.Equals("Rides are available")) {
                        return "Rides are available";
                    } else if (result.Equals("No available rides")) {
                        return "No available rides";
                    } else if ((result.Equals("Failure"))) {
                        return "error";
                    } else {
                        return "error";
                    }
                }
            } catch (Exception e) {
                KCApi.OutputException(e);
                return "Error Connecting to Server";
            }
            return "error";
        }
        
        //binds a driver to a ride.
        public static bool AcceptNextRide(Ride ride)
        {
            string message = "http://" + ip + "/driver/acceptRide.php?token=" + Driver_Id.token + "&driverID=" + Driver_Id.driver_Id;
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

                dynamic jObject = null;
                if (!responseFromServer.Contains("Failure")) {
                    jObject = JObject.Parse(responseFromServer);

                    ride.SetRideID(Int32.Parse((string)jObject.result.rideID));
                    ride.SetRidePhoneNum((string)jObject.result.phone);
                    ride.SetName((string)jObject.result.name);
                    return true;
                }
                //If the response comes back as Authentication failure then set the driver as not authenticated.
                else if (responseFromServer.Contains("Unable to authenticate") || responseFromServer.Contains("Authentication failure")) {
                    Driver_Id.authenticated = false;
                    return false;
                }
                
                return false;
            } catch (Exception e) {
                KCApi.OutputException(e);
                return false;
            }

        }
        
        //sets the clients postion in the ride class.
        public static bool SetRideLocation(Ride ride, double latitude, double longitude)
        {
            string message = "http://" + ip + "/driver/rideStatus.php?driverID=" + Driver_Id.driver_Id
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
                KCApi.OutputException(e);
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
                    //If there are no errors add the lat and long to the ride class and return true.\
                    dynamic jObject = JObject.Parse(responseFromServer);
                    
                    double rideLat = Double.Parse((String)jObject.result.lat);
                    double rideLong = Double.Parse((String)jObject.result.lon);

                    ride.SetPosition(rideLat, rideLong);
                    return true;
                }
                catch (Exception e)
                {
                    KCApi.OutputException(e);
                    return false;
                }
            }

            return false;
        }

        //Decouples the driver from a ride and completes it.
        public static bool CompleteRide(Ride ride)
        {
            string message = "http://" + ip + "/driver/completeRide.php?token=" + Driver_Id.token + "&driverID=" 
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

        //Sends the location of the driver to the client.
        public static bool SetDriverLocation(double latitude, double longitude)
        {
            string message = "http://" + ip + "/driver/updateLocation.php?driverID=" +  Driver_Id.driver_Id
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
                KCApi.OutputException(e);
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

        //Decouples the driver from the ride and puts the ride back in the queue.
        public static bool CancelRide(Ride ride)
        {
            string message = "http://" + ip + "/driver/decouple.php?" + "rideID=" + ride.RideId + "&token=" + Driver_Id.token +
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
                KCApi.OutputException(e);
                return false;
            }


            if (!responseFromServer.Contains("error"))
            {
                return true;
            }

            return false;
        }

        //If the driver already has a ride it sets the ride id to that ride.
        public static bool RecoveryCheck(Ride ride) {
            string message = "http://" + ip + "/driver/recoveryCheck.php?token=" + Driver_Id.token + "&driverID=" + Driver_Id.driver_Id;
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

                dynamic jObject = null;
                if (!responseFromServer.Contains("Failure")) {
                    jObject = JObject.Parse(responseFromServer);

                    ride.SetRideID(Int32.Parse((string)jObject.result.rideID));
                    ride.SetRidePhoneNum((string)jObject.result.phone);
                    ride.SetName((string)jObject.result.name);
                    return true;
                }
                //If the response comes back as Authentication failure then set the driver as not authenticated.
                else if (responseFromServer.Contains("Authentication failure")) {
                    Driver_Id.authenticated = false;
                    return false;
                }

                return false;
            } catch (Exception e) {
                KCApi.OutputException(e);
                return false;
            }
        }
    }
}
