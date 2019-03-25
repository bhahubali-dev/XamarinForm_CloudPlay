using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class RoundButton : Button
	{
		public static readonly BindableProperty TagProperty =
			BindableProperty.Create("Tag", typeof(int), typeof(RoundButton), 0);

		public int Tag
		{
			get { return (int)GetValue(TagProperty); }
			set { SetValue(TagProperty, value); }
		}
	}
}
