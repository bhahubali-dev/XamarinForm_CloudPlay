using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class SegmentedControl : View, IViewContainer<SegmentedControlOption>
	{
		public IList<SegmentedControlOption> Children { get; set; }
		public event ValueChangedEventHandler ValueChanged;
		public delegate void ValueChangedEventHandler(object sender, EventArgs e);

		public SegmentedControl()
		{
			Children = new List<SegmentedControlOption>();
		}

		public static BindableProperty SelectedValueProperty =
			BindableProperty.Create(nameof(SelectedValue), typeof(int), typeof(SegmentedControl),
			defaultValue: 0,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var segmentedControl = (SegmentedControl)bindable;
				segmentedControl.SelectedValue = (int)newValue;
				segmentedControl.ValueChanged?.Invoke(segmentedControl, EventArgs.Empty);
			});

		public int SelectedValue
		{
			get { return (int)GetValue(SelectedValueProperty); }
			set { SetValue(SelectedValueProperty, value); }
		}

		public static readonly BindableProperty FontSizeProperty =
			BindableProperty.Create("FontSize", typeof(double), typeof(SegmentedControl), 10.0);

		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create("BorderColor", typeof(Color), typeof(SegmentedControl), Color.FromHex("#3bb4ff"));

		public Color BorderColor
		{
			get { return (Color)base.GetValue(BorderColorProperty); }
			set { base.SetValue(BorderColorProperty, value); }
		}

		public static readonly BindableProperty BorderWidthProperty =
			BindableProperty.Create("BorderWidth", typeof(int), typeof(SegmentedControl), 2);

		public int BorderWidth
		{
			get { return (int)base.GetValue(BorderWidthProperty); }
			set { base.SetValue(BorderWidthProperty, value); }
		}

		public static readonly BindableProperty NormalColorProperty =
			BindableProperty.Create("NormalColor", typeof(Color), typeof(SegmentedControl), Color.Transparent);

		public Color NormalColor
		{
			get { return (Color)base.GetValue(NormalColorProperty); }
			set { base.SetValue(NormalColorProperty, value); }
		}

		public static readonly BindableProperty SelectedColorProperty =
			BindableProperty.Create("SelectedColor", typeof(Color), typeof(SegmentedControl), Color.FromHex("#3bb4ff"));

		public Color SelectedColor
		{
			get { return (Color)base.GetValue(SelectedColorProperty); }
			set { base.SetValue(SelectedColorProperty, value); }
		}

		public static readonly BindableProperty NormalTextColorProperty =
			BindableProperty.Create("NormalTextColor", typeof(Color), typeof(SegmentedControl), Color.FromHex("#3bb4ff"));

		public Color NormalTextColor
		{
			get { return (Color)base.GetValue(NormalTextColorProperty); }
			set { base.SetValue(NormalTextColorProperty, value); }
		}

		public static readonly BindableProperty SelectedTextColorProperty =
			BindableProperty.Create("SelectedTextColor", typeof(Color), typeof(SegmentedControl), Color.White);

		public Color SelectedTextColor
		{
			get { return (Color)base.GetValue(SelectedTextColorProperty); }
			set { base.SetValue(SelectedTextColorProperty, value); }
		}

		public static readonly BindableProperty BorderRadiusProperty =
			BindableProperty.Create("BorderRadius", typeof(int), typeof(SegmentedControl), 8);

		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}
	}

	public class SegmentedControlOption : View
	{
		public SegmentedControlOption()
		{
		}

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SegmentedControl), "");

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty NormalImageProperty = BindableProperty.Create("NormalImage", typeof(string), typeof(SegmentedControl), "");

		public string NormalImage
		{
			get { return (string)GetValue(NormalImageProperty); }
			set { SetValue(NormalImageProperty, value); }
		}

		public static readonly BindableProperty SelectedImageProperty = BindableProperty.Create("SelectedImage", typeof(string), typeof(SegmentedControl), "");

		public string SelectedImage
		{
			get { return (string)GetValue(SelectedImageProperty); }
			set { SetValue(SelectedImageProperty, value); }
		}
	}
}
