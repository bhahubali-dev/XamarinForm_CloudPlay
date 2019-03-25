using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlayOnCloud
{
	//XXX: WTF this does not belong here or as a base of any of these shared models
	public class NotifyPropertyChanged : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			OnPropertyChanged(propertyName);

			return true;
		}
	}
}
