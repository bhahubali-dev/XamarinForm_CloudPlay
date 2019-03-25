using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class SlidingGrid : Grid
	{
		public SlidingGrid()
		{
			this.SetValue(RelativeLayout.XConstraintProperty, Constraint.RelativeToParent((p) => { return (Parent as VisualElement).Width - (this.Shown ? this.Width : 0); }));
		}

		public static BindableProperty ShownProperty =
			BindableProperty.Create("Shown", typeof(bool), typeof(SlidingGrid),
			defaultValue: false,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				if (oldValue != newValue)
				{
					var ctrl = (SlidingGrid)bindable;
					ctrl.Shown = (bool)newValue;
					ctrl.Animate((bool)newValue);
				}
			});

		public bool Shown
		{
			get { return (bool)GetValue(ShownProperty); }
			set { SetValue(ShownProperty, value); }
		}

		public void Animate(bool show)
		{
			Device.BeginInvokeOnMainThread(async () =>
			{
				if (show)
				{
					var rect = new Rectangle((Parent as VisualElement).Width - this.Width, 0, this.Width, this.Height);
					await this.LayoutTo(rect, 0, Easing.Linear);
				}
				else
				{
					var rect = new Rectangle((Parent as VisualElement).Width, 0, this.Width, this.Height);
					await this.LayoutTo(rect, 0, Easing.Linear);
				}
			});
		}
	}
}
