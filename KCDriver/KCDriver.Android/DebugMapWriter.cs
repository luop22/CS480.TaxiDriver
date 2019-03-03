using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Maps;

namespace KCDriver.Droid
{
    class DebugMapWriter
    {
        /// <summary>
        /// Outputs a GPX file for use in an emulator.
        /// The file holds a series of gps coordinates and
        /// can crudely simulate movement.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public string OutputGPX(List<Position> route)
        {
            if (route.Count == 0)
                return "";

            string output = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\n"
            + "<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" creator=\"MapSource 6.16.1\"\n"
            + "version=\"1.1\" xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-"
            + "instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">\n\n";

            output += "<trk>\n"
            + "<name>emulate</name>\n"
            + "<trkseg>";
            int seconds = 0, minutes = 0;
            foreach (Position p in route)
            {
                if (seconds == 59)
                {
                    seconds = 0;
                    ++minutes;
                }
                else ++seconds;

                output += "\n<trkpt lat=\"" + p.Latitude + "\" lon=\"" + p.Longitude + "\"><ele>0.000000</ele>"
                    + "<time>2014-03-05T20:";
                if (minutes < 10)
                    output += "0";

                output += minutes.ToString() + ":";

                if (seconds < 10)
                    output += "0";

                output += seconds.ToString() + "Z</time></trkpt>";
            }

            output += "\n</trkseg>"
                + "\n</trk>"
                + "\n</gpx> ";

            System.Diagnostics.Debug.Write(output);
            return output;
        }
    }
}