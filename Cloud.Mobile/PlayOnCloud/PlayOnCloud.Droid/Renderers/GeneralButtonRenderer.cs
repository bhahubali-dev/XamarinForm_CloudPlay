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
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using Android.Graphics.Drawables;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.Button), typeof(PlayOnCloud.Droid.Renderers.GeneralButtonRenderer))]
namespace PlayOnCloud.Droid.Renderers
{
	class GeneralButtonRenderer : ButtonRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
		{
			base.OnElementChanged(e);

			if ((Control != null) && (e.NewElement != null))
			{
				var button = (Android.Widget.Button)this.Control;
				button.SetAllCaps(false);
				button.TextAlignment = TextAlignment.Center;
				button.SetPadding(0,0,0,0);

				if (Element.Image != null)
					button.Background = new ColorDrawable(Color.Transparent);
			}
		}

		public override void ChildDrawableStateChanged(View child)
		{
			base.ChildDrawableStateChanged(child);
			Control.Text = Control.Text;
		}
	}
}