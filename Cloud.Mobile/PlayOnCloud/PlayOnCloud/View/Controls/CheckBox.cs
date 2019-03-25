using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class CheckBox : View
	{
		public static readonly BindableProperty CheckedProperty =
			BindableProperty.Create("Checked", typeof(bool), typeof(CheckBox), false, BindingMode.TwoWay, propertyChanged: OnCheckedPropertyChanged);

		public static readonly BindableProperty CheckedImageProperty = BindableProperty.Create("CheckedImage", typeof(string), typeof(CheckBox), string.Empty);
		public static readonly BindableProperty UnCheckedImageProperty = BindableProperty.Create("UnCheckedImage", typeof(string), typeof(CheckBox), string.Empty);

		public event EventHandler<EventArgs<bool>> CheckedChanged;

		public bool Checked
		{
			get { return (bool)GetValue(CheckedProperty); }
			set
			{
				if (this.Checked != value)
				{
					this.SetValue(CheckedProperty, value);
					if (CheckedChanged != null)
						this.CheckedChanged.Invoke(this, new EventArgs<bool>(value));
				}
			}
		}

		private static void OnCheckedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var checkBox = (CheckBox)bindable;
			checkBox.Checked = (bool)newvalue;
		}

		public string CheckedImage
		{
			get { return (string)GetValue(CheckedImageProperty); }
			set { SetValue(CheckedImageProperty, value); }
		}

		public string UnCheckedImage
		{
			get { return (string)GetValue(UnCheckedImageProperty); }
			set { SetValue(UnCheckedImageProperty, value); }
		}
	}
}
