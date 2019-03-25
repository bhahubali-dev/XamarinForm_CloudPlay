using Xamarin.Forms;

namespace PlayOnCloud
{
	public class SlidingContentView : ContentView
	{
		public static BindableProperty DetailsShownProperty =
			BindableProperty.Create("DetailsShown", typeof(bool), typeof(SlidingContentView),
			defaultValue: false,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				if (oldValue != newValue)
				{
					var ctrl = (SlidingContentView)bindable;
					ctrl.DetailsShown = (bool)newValue;
					ctrl.Animate((bool)newValue);
				}
			});

		public static BindableProperty DetailsWidthProperty = BindableProperty.Create("DetailsWidth", typeof(double), typeof(SlidingContentView), defaultValue: 0.0);

		public bool DetailsShown
		{
			get { return (bool)GetValue(DetailsShownProperty); }
			set { SetValue(DetailsShownProperty, value); }
		}

		public double DetailsWidth
		{
			get { return (double)GetValue(DetailsWidthProperty); }
			set { SetValue(DetailsWidthProperty, value); }
		}

		public async void Animate(bool detailsShown)
		{
			if (detailsShown)
			{
				var rect = new Rectangle(0, 0, this.Width - DetailsWidth, this.Height);
				await this.LayoutTo(rect, 0, Easing.Linear);
			}
			else
			{
				var rect = new Rectangle(0, 0, (Parent as VisualElement).Width, this.Height);
				await this.LayoutTo(rect, 0, Easing.Linear);
			}
		}
	}
}
