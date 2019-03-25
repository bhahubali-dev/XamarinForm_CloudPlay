using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class PagerIndicatorDots : StackLayout
	{
		int selectedIndex;

		public Color DotColor { get; set; }
		public double DotSize { get; set; }

		public PagerIndicatorDots()
		{
			HorizontalOptions = LayoutOptions.CenterAndExpand;
			VerticalOptions = LayoutOptions.Center;
			Orientation = StackOrientation.Horizontal;
			DotColor = Color.Black;
		}

		void CreateDot()
		{
			var dot = new Button
			{
				BorderRadius = Convert.ToInt32(DotSize / 2),
				HeightRequest = DotSize,
				WidthRequest = DotSize,
				BackgroundColor = DotColor
			};

			Children.Add(dot);
		}

		public static BindableProperty ItemsSourceProperty =
			BindableProperty.Create(
				"ItemsSource", typeof(IList), typeof(PagerIndicatorDots),
				null,
				BindingMode.OneWay,
				propertyChanging: (bindable, oldValue, newValue) =>
				{
					((PagerIndicatorDots)bindable).ItemsSourceChanging();
				},
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					((PagerIndicatorDots)bindable).ItemsSourceChanged();
				}
		);

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static BindableProperty SelectedItemProperty =
			BindableProperty.Create(
				"SelectedItem", typeof(object), typeof(PagerIndicatorDots),
				null,
				BindingMode.TwoWay,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					((PagerIndicatorDots)bindable).SelectedItemChanged();
				});

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		void ItemsSourceChanging()
		{
			if (ItemsSource != null)
				selectedIndex = ItemsSource.IndexOf(SelectedItem);
		}

		void ItemsSourceChanged()
		{
			if (ItemsSource == null)
				return;

			var countDelta = ItemsSource.Count - Children.Count;

			if (countDelta > 0)
				for (var i = 0; i < countDelta; i++)
					CreateDot();
			else if (countDelta < 0)
				for (var i = 0; i < -countDelta; i++)
					Children.RemoveAt(0);
		}

		void SelectedItemChanged()
		{
			var selectedIndex = ItemsSource.IndexOf(SelectedItem);
			var pagerIndicators = Children.Cast<Button>().ToList();

			foreach (var pi in pagerIndicators)
				UnselectDot(pi);

			if (selectedIndex > -1)
				SelectDot(pagerIndicators[selectedIndex]);
		}

		static void UnselectDot(Button dot)
		{
			dot.Opacity = 0.5;
		}

		static void SelectDot(Button dot)
		{
			dot.Opacity = 1.0;
		}
	}
}
