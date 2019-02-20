using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

//class which handles calls to the server including authentication and ride requestes
namespace KCDriver {
    class ServerRequests {

        //authentication functions
        //returns true if the user is authenticated with the server
        public Droid.Driver_Id Authenticate(String password, String userName) {

            String message = "http://148.72.40.62/driver/auth/authenticate.php?username=" + userName + "&pwHsh=" + GetHash(password, userName);
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

            if (!responseFromServer.Contains("error")) {
                String[] data = responseFromServer.Split(new char[] { '"', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

                return new Droid.Driver_Id(Int32.Parse(data[4]), data[6]);
            } else {
                return null;
            }
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
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            if (!responseFromServer.Contains("error")) {
                String[] data = responseFromServer.Split(new char[] { '"', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
                return data[4];
            } else {
                return "error";
            }
        }
    }
}
