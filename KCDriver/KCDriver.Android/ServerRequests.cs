using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

/* Contains functions for communicating with the remote database. */

namespace KCDriver.Droid {

    static partial class KCApi {

        private const string ip = "kccabapp.com";
        private const int timeout = 5000;

        /// <summary>
        /// Attempts to authenticate the user. Returns true if the user 
        /// is authenticated with the server, false upon error or rejection.
        /// </summary>
        /// <param name="password">Password to include in the query.</param>
        /// <param name="userName">Username to include in the query.</param>
        /// <returns></returns>
        public static bool Authenticate(string password, string userName)
        {
            string message = "https://" + ip + "/driver/auth/authenticate.php?username=" + userName + "&pwHsh=" + GetHash(password, userName);
            string responseFromServer = "";
            try
            {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                request.Timeout = timeout;
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

                if (!responseFromServer.Contains("Failure"))
                {
                    jObject = JObject.Parse(responseFromServer);

                    Driver_Id.driver_Id = Int32.Parse((string)jObject.result.driverID);
                    Driver_Id.token = (string)jObject.result.token;
                    Driver_Id.authenticated = true;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return false;
            }             
        }

        /// <summary>
        /// Calculates the hash of the password with the salt.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        private static string GetHash(string input, string userName)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                HashAlgorithm hashAlgorithm = sha256Hash;
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(GetSalt(userName) + input));

                // Create a new stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// Gets the salt for a user in order to hash their password.
        /// </summary>
        /// <param name="userName">Username from which to get the salt.</param>
        /// <returns>string containing the salt.</returns>
        public static string GetSalt(string userName)
        {
            string message = "https://" + ip + "/driver/auth/getSalt.php?username=" + userName;
            string responseFromServer = "";
            try
            {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                request.Timeout = timeout;
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return "No Internet connection.";
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return "Error Connecting to Server";
            } 
        }

        /// <summary>
        /// Checks if there are any rides in the queue. Returns a
        /// string which represents the status of the server.
        /// </summary>
        /// <returns>String containing the status of the queue.</returns>
        public static string CheckQueue()
        {
            string message = "https://" + ip + "/driver/checkQueue.php";
            string responseFromServer = "";
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            request.Timeout = timeout;

            try
            {
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
                    string result = (string)jObject.result;
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return "No Internet connection.";
            }
            catch (Exception e) {
                KCApi.OutputException(e);
                return "Error Connecting to Server";
            }

            return "error";
        }

        /// <summary>
        /// Takes a Ride ref and binds a driver to the ride on the remote server.
        /// </summary>
        /// <param name="ride">The ride object to store the active ride info in.</param>
        /// <returns>True if a ride is accepted, false otherwise.</returns>
        public static bool AcceptNextRide(Ride ride)
        {
            string message = "https://" + ip + "/driver/acceptRide.php?token=" + Driver_Id.token + "&driverID=" + Driver_Id.driver_Id;
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            request.Timeout = timeout;
            // Get the response.
            string responseFromServer = "";

            try
            {
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return false;
            }

        }

        /// <summary>
        /// Sets the clients postion in the given Ride by querying
        /// the remote server
        /// </summary>
        /// <param name="ride">The ride to store the location retrieved in.</param>
        /// <param name="latitude">Current latitude of the driver.</param>
        /// <param name="longitude">Current longitude of the driver.</param>
        /// <returns>True upon success, false upon failure</returns>
        public static bool SetRideLocation(Ride ride, double latitude = 0, double longitude = 0)
        {
            string message = "https://" + ip + "/driver/rideStatus.php?driverID=" + Driver_Id.driver_Id
                + "&token=" + Driver_Id.token + "&rideID=" + ride.RideId;

            if (Properties.CurrentPosition.Latitude != 0 
                || Properties.CurrentPosition.Longitude != 0)
                message += "&lat=" + latitude + "&lon=" + longitude;

            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            request.Timeout = timeout;
            // Get the response.
            string responseFromServer = "";
            try
            { 
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return false;
            }

            //If the response comes back as Authentication failure then set the driver as not authenticated.
            if (responseFromServer.Contains("Authentication failure") || responseFromServer.Contains("Unable to authenticate")) {
                Driver_Id.authenticated = false;
                return false;
            }
            // If the response is "Ride not found", the user has canceled.
            else if (responseFromServer.Contains("Ride not found"))
            {
                Properties.RideStatus = KCProperties.RideStatuses.CanceledByRider;
            }
            //check if there are any other errors
            else if (!responseFromServer.Contains("error"))
            {
                try
                {
                    //If there are no errors add the lat and long to the ride class and return true.\
                    dynamic jObject = JObject.Parse(responseFromServer);
                    
                    double rideLat = Double.Parse((string)jObject.result.lat);
                    double rideLong = Double.Parse((string)jObject.result.lon);

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

        /// <summary>
        /// Decouples the driver from a ride and completes it by querying
        /// the remote server.
        /// </summary>
        /// <param name="ride">Ride object to complete.</param>
        /// <returns>True if a success response was recieved, false otherwise.</returns>
        public static bool CompleteRide(Ride ride)
        {
            string message = "https://" + ip + "/driver/completeRide.php?token=" + Driver_Id.token + "&driverID=" 
                + Driver_Id.driver_Id + "&rideID=" + ride.RideId;

            try
            {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                request.Timeout = timeout;
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
            }

            return false;
        }

        /// <summary>
        /// Updates the remote server on the current location of the driver without getting ride info.
        /// </summary>
        /// <param name="latitude">Current latitude of the driver.</param>
        /// <param name="longitude">Current longitude of the driver.</param>
        /// <returns></returns>
        public static bool SetDriverLocation(double latitude, double longitude)
        {
            string message = "https://" + ip + "/driver/updateLocation.php?driverID=" +  Driver_Id.driver_Id
                + "&token=" + Driver_Id.token + "&lat=" + latitude + "&lon=" + longitude;
            string responseFromServer = "";

            try
            {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create(message);
                request.Timeout = timeout;
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
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

        /// <summary>
        /// Decouples the driver from the ride and puts the ride back in the queue.
        /// </summary>
        /// <param name="ride">Ride object to cancel.</param>
        /// <returns>True upon receipt of a success response, false otherwise.</returns>
        public static bool CancelRide(Ride ride)
        {
            string message = "https://" + ip + "/driver/decouple.php?" + "rideID=" + ride.RideId + "&token=" + Driver_Id.token +
                 "&driverID=" + Driver_Id.driver_Id;
            string responseFromServer = "";
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            request.Timeout = timeout;

            try
            {
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return false;
            }


            if (!responseFromServer.Contains("error"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// If the driver already has a ride it sets the ride id to that ride
        /// in case of failure or lost connection.
        /// </summary>
        /// <param name="ride">Ride object to store recovery info in.</param>
        /// <returns>True upon receipt of a success response, false otherwise.</returns>
        public static bool RecoveryCheck(Ride ride) {
            string message = "https://" + ip + "/driver/recoveryCheck.php?token=" + Driver_Id.token + "&driverID=" + Driver_Id.driver_Id;
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(message);
            request.Timeout = timeout;

            // Get the response.
            string responseFromServer = "";
            try
            {
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
                if (!responseFromServer.Contains("Failure") && !responseFromServer.Contains("No")) {
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
            }
            catch (WebException)
            {
                lock (Properties.NetworkStateLock)
                    KCApi.Properties.NetState = KCProperties.NetworkState.Disconnected;

                return false;
            }
            catch (Exception e)
            {
                KCApi.OutputException(e);
                return false;
            }
        }
    }
}
