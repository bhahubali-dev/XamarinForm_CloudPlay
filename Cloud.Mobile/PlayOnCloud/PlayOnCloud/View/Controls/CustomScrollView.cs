using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class CustomScrollView : ScrollView
	{
		public static readonly BindableProperty IsRefreshingProperty =
			BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(CustomScrollView), false);

		public static readonly BindableProperty DisableBouncesProperty =
			BindableProperty.Create(nameof(DisableBounces), typeof(bool), typeof(CustomScrollView), false);

		public static readonly BindableProperty RefreshCommandProperty =
			BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(CustomScrollView), null);

		public bool IsRefreshing
		{
			get { return (bool)GetValue(IsRefreshingProperty); }
			set { SetValue(IsRefreshingProperty, value); }
		}

		public bool DisableBounces
		{
			get { return (bool)GetValue(DisableBouncesProperty); }
			set { SetValue(DisableBouncesProperty, value); }
		}

		public ICommand RefreshCommand
		{
			get { return (ICommand)GetValue(RefreshCommandProperty); }
			set { SetValue(RefreshCommandProperty, value); }
		}
	}
}
