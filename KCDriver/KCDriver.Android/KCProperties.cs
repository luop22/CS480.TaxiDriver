using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Maps;
using KCDriver.Droid;

namespace KCDriver.Droid
{
    // Generic property code here: https://stackoverflow.com/questions/2246777/raise-an-event-whenever-a-propertys-value-changed
    public class KCProperties : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void SetPropertyField<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // The Map object which holds the google map with thread-safe get and set.
        // Thread lock
        private static readonly object mapLock = new object();
        // Private-facing
        private KCMap map;
        // Public facing
        public KCMap Map
        {
            get
            {
                // Thread safe get
                lock (mapLock)
                {
                    return map;
                }
            }

            set
            {
                lock (mapLock)
                {
                    map = value;
                }
            }
        }

        // Custom renderer object used for drawing on the map
        private readonly object renderLock = new object();
        private KCMapRenderer renderer;
        public KCMapRenderer Renderer
        {
            get
            {
                lock (renderLock)
                {
                    return renderer;
                }
            }

            set
            {
                lock (renderLock)
                {
                    renderer = value;
                }
            }
        }

        // Coordinates of the current route to be drawn.
        // This particular property shoots events when it is changed. See KCMapRenderer for catching
        private readonly object routeLock = new object();
        private List<Position> routeCoordinates;
        public List<Position> RouteCoordinates
        {
            get
            {
                lock (routeLock)
                {
                    return routeCoordinates;
                }
            }

            set
            {
                lock (routeLock)
                {
                    // Outputs a GPX map based on the new route.
                    /*DebugMapWriter dmw = new DebugMapWriter();
                    var s = dmw.OutputGPX(value);*/
                    SetPropertyField("RouteCoordinates", ref routeCoordinates, value);
                }
            }
        }

        // Single lock for values tracking when the map and renderer call OnReady()
        private readonly object boolLock = new object();
        private bool mapReady;
        public bool MapReady
        {
            get
            {
                lock (boolLock)
                {
                    return mapReady;
                }
            }

            set
            {
                lock (boolLock)
                {
                    mapReady = value;
                }
            }
        }

        // See above
        private bool renderReady;
        public bool RenderReady
        {
            get
            {
                lock (boolLock)
                {
                    return renderReady;
                }
            }

            set
            {
                lock (boolLock)
                {
                    renderReady = value;
                }
            }
        }

        // Current GPS position
        private readonly object positionLock = new object();
        private Position currentPosition;
        public Position CurrentPosition
        {
            get
            {
                lock (positionLock)
                {
                    return currentPosition;
                }
            }

            internal set
            {
                lock (positionLock)
                {
                    // This block of code is supposed to establish the speed the user is going
                    // using elapsed time and distance
                    if (PositionTimer == null) PositionTimer = new System.Diagnostics.Stopwatch();
                    if (PositionTimer.IsRunning)
                    {
                        PositionTimer.Stop();
                        SpeedTime = PositionTimer.ElapsedMilliseconds;
                        PositionTimer.Restart();
                    }
                    else PositionTimer.Start();

                    PreviousPosition = currentPosition;
                    SetPropertyField("CurrentPosition", ref currentPosition, value);
                }
            }
        }

        // Previous position for calculating speed
        private readonly object prevPositionLock = new object();
        private Position previousPosition;
        public Position PreviousPosition
        {
            get
            {
                lock (prevPositionLock)
                {
                    return previousPosition;
                }
            }

            set
            {
                lock (prevPositionLock)
                {
                    previousPosition = value;
                }
            }
        }

        // Tracks smoothed/interpolated position
        private readonly object interpolatedPositionLock = new object();
        private Position interpolatedPosition;
        public Position InterpolatedPosition
        {
            get
            {
                lock (interpolatedPositionLock)
                {
                    return interpolatedPosition;
                }
            }

            set
            {
                lock (interpolatedPositionLock)
                {
                    SetPropertyField("InterpolatedPosition", ref interpolatedPosition, value);
                }
            }
        }

        // Timer for calculating speed
        private readonly object positionTimerLock = new object();
        private System.Diagnostics.Stopwatch positionTimer;
        public System.Diagnostics.Stopwatch PositionTimer
        {
            get
            {
                lock (positionTimerLock)
                {
                    return positionTimer;
                }
            }

            set
            {
                lock (positionTimerLock)
                {
                    positionTimer = value;
                }
            }
        }

        // elapsed ms of last cycle
        private readonly object speedTimeLock = new object();
        private double speedTime;
        public double SpeedTime
        {
            get
            {
                lock (positionTimerLock)
                {
                    return speedTime;
                }
            }

            set
            {
                lock (positionTimerLock)
                {
                    speedTime = value;
                }
            }
        }
    }
}
