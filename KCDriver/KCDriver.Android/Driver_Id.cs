using System;

namespace KCDriver.Droid {

    /// <summary>
    /// Class which stores in formation about the current logged in driver.
    /// </summary>
    public static class Driver_Id {

        public static int driver_Id { get; set; }
        public static String token { get; set; }
        public static bool authenticated { get; set; }

    }
}