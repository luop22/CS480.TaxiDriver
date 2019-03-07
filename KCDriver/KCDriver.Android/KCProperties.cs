using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Xamarin.Forms.Maps;

namespace KCDriver.Droid {
    // Generic property code here: https://stackoverflow.com/questions/2246777/raise-an-event-whenever-a-propertys-value-changed
    public class KCProperties : INotifyPropertyChanged
    {
        /// <summary>
        /// Implementation for the INotifyPropertyChanged interface.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Sets the field to the value and notifies all subscribed 
        /// instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
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

        #region AppState
        public enum AppState
        {
            SignIn,
            Accept,
            Map,
            Transitioning
        };

        private readonly object stateLock = new object();
        public readonly object StateLock = new object(); // For external classes to coordinate state changes
        private AppState state;
        public AppState State
        {
            get
            {
                lock (stateLock)
                {
                    return state;
                }
            }

            set
            {
                lock (stateLock)
                {
                    state = value;
                }
            }
        }
        #endregion

        #region NetworkState
        public enum NetworkState
        {
            Connected,
            Retrying,
            Disconnected
        }

        private readonly object networkStateLock = new object();
        public readonly object NetworkStateLock = new object();
        private int netStateTimeout = 30000;
        private Stopwatch netStateTimer = new Stopwatch();
        private NetworkState netState = NetworkState.Connected;
        public NetworkState NetState
        {
            get
            {
                lock (networkStateLock)
                {
                    return netState;
                }
            }

            set
            {
                lock (networkStateLock)
                {
                    if (value == NetworkState.Disconnected)
                    {
                        if (netStateTimer.ElapsedMilliseconds >= netStateTimeout
                            && netState == NetworkState.Retrying)
                        {
                            netStateTimer.Reset();
                            SetPropertyField("NetState", ref netState, NetworkState.Disconnected);
                        }
                        else if (!netStateTimer.IsRunning)
                        {
                            netState = NetworkState.Retrying;
                            netStateTimer.Start();
                        }
                    }
                    else
                    {
                        netStateTimer.Reset();
                        netState = value;
                    }
                }
            }
        }
        #endregion

        /* Slated for removal
        #region GPSState
        public enum GPSState
        {
            Connected,
            Disconnected
        }

        private readonly object gpsStateLock = new object();
        public readonly object GPSStateLock = new object();
        private GPSState gpsState;
        public GPSState GpsState
        {
            get
            {
                lock (gpsStateLock)
                {
                    return gpsState;
                }
            }

            set
            {
                lock (gpsStateLock)
                {
                    gpsState = value;
                }
            }
        }
        #endregion */

        // The Map object which holds the google map with thread-safe get and set.
        #region Map
        private readonly object mapLock = new object();
        private KCMap map;
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
        #endregion

        // Custom renderer object used for drawing on the map
        #region Renderer
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
        #endregion

        #region CurrentRide
        private readonly object rideLock = new object();
        private Ride currentRide;
        public Ride CurrentRide
        {
            get
            {
                lock (rideLock)
                {
                    return currentRide;
                }
            }

            set
            {
                lock (rideLock)
                {
                    SetPropertyField("CurrentRide", ref currentRide, value);
                }
            }
        }
        #endregion

        // Single lock for values tracking when the map and renderer call OnReady()
        #region MapReady
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
        #endregion

        // See above
        #region RenderReady
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
        #endregion

        // Current GPS position
        #region CurrentPosition
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
                    SetPropertyField("CurrentPosition", ref currentPosition, value);
                }
            }
        }
        #endregion

        // To track when permissions are being asked
        #region Permission Flow
        private readonly object permissionLock = new object();
        private bool askingLocationPermission;
        public bool AskingLocationPermission
        {
            get
            {
                lock (permissionLock)
                {
                    return askingLocationPermission;
                }
            }

            set
            {
                lock (permissionLock)
                {
                    askingLocationPermission = value;
                }
            }
        }
        private bool haveLocationPermission;
        public bool HaveLocationPermission
        {
            get
            {
                lock (permissionLock)
                {
                    return haveLocationPermission;
                }
            }

            set
            {
                lock (permissionLock)
                {
                    haveLocationPermission = value;
                }
            }
        }
        #endregion

        // Track which camera lock is active if any
        #region Camera Controls
        private readonly object cameraDriverLock = new object();
        private bool cameraOnDriver;
        public bool CameraOnDriver
        {
            get
            {
                lock (cameraDriverLock)
                {
                    return cameraOnDriver;
                }
            }

            set
            {
                lock (cameraDriverLock)
                {
                    if (value == true)
                        CameraOnRider = false;
                    cameraOnDriver = value;
                }
            }
        }

        private readonly object cameraRiderLock = new object();
        private bool cameraOnRider;
        public bool CameraOnRider
        {
            get
            {
                lock (cameraRiderLock)
                {
                    return cameraOnRider;
                }
            }

            set
            {
                lock (cameraRiderLock)
                {
                    if (value == true)
                        CameraOnDriver = false;
                    cameraOnRider = value;
                }
            }
        }
        #endregion

        // Track the rides status
        #region RideActive
        private readonly object rideActiveLock = new object();
        private bool rideActive;
        public bool RideActive
        {
            get
            {
                lock (rideActiveLock)
                {
                    return rideActive;
                }
            }

            set
            {
                lock (rideActiveLock)
                {
                    SetPropertyField("RideActive", ref rideActive, value);
                }
            }
        }
        #endregion
    }
}
