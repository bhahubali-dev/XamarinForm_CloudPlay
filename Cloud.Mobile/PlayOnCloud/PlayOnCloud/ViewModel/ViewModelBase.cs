using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public class ViewModelBase : NotifyPropertyChanged
    {
        private readonly object isInBackgroundLock = new object();
        private NetworkStatus currentNetworkStatus;
        private DeviceOrientation deviceOrientation = DeviceOrientation.Landscape;
        private bool initialized;
        private bool isDetailsLoading;
        private bool isInBackground = true;
        private bool isLoading;
        private bool isRefreshing;
        private int loadingCount;
        private int loadingDetailsCount;

        public ViewModelBase()
        {
            Refresh = new Command(async () => await refresh(false));
            ListViewItemSelected = new Command(() => Fire_OnSelectedItemDetailsChanged());
            CurrentNetworkStatus = ReachabilityHelperService.Instance.InternetConnectionStatus;
        }

        public ICommand Refresh { protected set; get; }

        public ICommand ListViewItemSelected { protected set; get; }

        protected bool SuspendOnSelectedItemDetailsChangedEvent { get; set; }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                var res = loadingCount;

                if (value)
                    res = Interlocked.Increment(ref loadingCount);
                else if (loadingCount > 0)
                    res = Interlocked.Decrement(ref loadingCount);

                SetField(ref isLoading, res != 0);
            }
        }

        public bool IsDetailsLoading
        {
            get { return isDetailsLoading; }
            set
            {
                var res = loadingDetailsCount;

                if (value)
                    res = Interlocked.Increment(ref loadingDetailsCount);
                else if (loadingDetailsCount > 0)
                    res = Interlocked.Decrement(ref loadingDetailsCount);

                SetField(ref isDetailsLoading, res != 0);
            }
        }

        public bool Initialized
        {
            get { return initialized; }
            set { SetField(ref initialized, value); }
        }

        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set { SetField(ref isRefreshing, value); }
        }

        public NetworkStatus CurrentNetworkStatus
        {
            get { return currentNetworkStatus; }
            set { SetField(ref currentNetworkStatus, value); }
        }

        public bool IsInBackground
        {
            get
            {
                lock (isInBackgroundLock)
                {
                    return isInBackground;
                }
            }

            set
            {
                lock (isInBackgroundLock)
                {
                    SetField(ref isInBackground, value);
                }
            }
        }

        public virtual DeviceOrientation DeviceOrientation
        {
            get { return deviceOrientation; }
            set { SetField(ref deviceOrientation, value); }
        }

        public TaskQueue TaskQueue { get; } = new TaskQueue();

        public event EventHandler OnSelectedItemDetailsChanged;

        public virtual async Task Init()
        {
            Initialized = true;
            await Task.Delay(0);
        }

        public virtual Task OnBackButtonPressed()
        {
            return Task.FromResult(default(object));
        }

        public virtual async Task Poll(bool showLoading)
        {
            await Task.Delay(0);
        }

        public virtual async Task Logout()
        {
            Initialized = false;
            await Task.Delay(0);
        }

        protected virtual async Task refresh(bool silent)
        {
            IsRefreshing = false;
            await Task.Delay(0);
        }

        public virtual async Task NetworkStatusChanged(NetworkStatus newStatus)
        {
            currentNetworkStatus = newStatus;
            await Task.Delay(0);
        }

        protected void Fire_OnSelectedItemDetailsChanged()
        {
            if (!SuspendOnSelectedItemDetailsChangedEvent)
            {
                var onSelectedItemDetailsChanged = OnSelectedItemDetailsChanged;
                if (onSelectedItemDetailsChanged != null)
                    onSelectedItemDetailsChanged(this, null);
            }
        }
    }
}