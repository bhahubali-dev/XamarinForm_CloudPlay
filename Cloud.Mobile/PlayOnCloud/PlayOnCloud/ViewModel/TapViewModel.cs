using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud.ViewModel
{
    public class TapViewModel : INotifyPropertyChanged
    {
        private int taps;

        public TapViewModel()
        {
            // configure the TapCommand with a method
            TapCommand = new Command(OnTapped);
        }

        public ICommand TapCommand { get; }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }

            remove { throw new NotImplementedException(); }
        }

        private void OnTapped(object s)
        {
            taps++;
            Debug.WriteLine("parameter: " + s);
            Device.OpenUri(new Uri(s.ToString()));
        }
    }
}