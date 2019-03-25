using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public class RoundFrame : Frame
	{
		public static readonly BindableProperty BorderRadiusProperty =
			BindableProperty.Create("BorderRadius", typeof(int), typeof(RoundFrame), 0);

		public static readonly BindableProperty BorderWidthProperty =
			BindableProperty.Create("BorderWidth", typeof(int), typeof(RoundFrame), 1);

		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create("BorderColor", typeof(Color), typeof(RoundFrame), Color.Transparent);

		public static readonly BindableProperty TagProperty =
			BindableProperty.Create("Tag", typeof(int), typeof(RoundFrame), 0);

		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.Create("ShadowColor", typeof(Color), typeof(RoundFrame), Color.Transparent);

		public static readonly BindableProperty ShadowRadiusProperty =
			BindableProperty.Create("ShadowRadius", typeof(float), typeof(RoundFrame), 0f);

		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}

		public int BorderWidth
		{
			get { return (int)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

		public int Tag
		{
			get { return (int)GetValue(TagProperty); }
			set { SetValue(TagProperty, value); }
		}

		public Color ShadowColor
		{
			get { return (Color)GetValue(ShadowColorProperty); }
			set { SetValue(ShadowColorProperty, value); }
		}

		public float ShadowRadius
		{
			get { return (float)GetValue(ShadowRadiusProperty); }
			set { SetValue(ShadowRadiusProperty, value); }
		}
	}
}
