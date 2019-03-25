using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(PlayOnCloud.Droid.Renderers.GeneralEntryRenderer))]
namespace PlayOnCloud.Droid.Renderers
{
	class GeneralEntryRenderer : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if ((Control != null) && (e.NewElement != null))
				Control.Background = null;
		}
	}
}