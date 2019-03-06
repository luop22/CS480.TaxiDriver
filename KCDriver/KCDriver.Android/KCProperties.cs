using System.Collections.Generic;
using System.ComponentModel;
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

        // The Map object which holds the google map with thread-safe get and set.
        // Thread lock
        private readonly object mapLock = new object();
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
                    SetPropertyField("CurrentPosition", ref currentPosition, value);
                }
            }
        }

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
    }
}
