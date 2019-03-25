using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class NavigationFrame : RoundFrame
	{
		private static double defaultImageGridHolderSize = ((Device.Idiom == TargetIdiom.Tablet) ? 21d : 15d);

		public event EventHandler OnClicked;

		public NavigationFrame()
		{
			InitializeComponent();

			this.Opacity = UncheckedAlpha;
			BadgeImageSource.IsVisible = false;

			imageGridHolder.WidthRequest = defaultImageGridHolderSize;
			imageGridHolder.HeightRequest = defaultImageGridHolderSize;
		}

		public static BindableProperty TextProperty =
			BindableProperty.Create("Text", typeof(string), typeof(NavigationFrame),
			defaultValue: string.Empty,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.Text = (string)newValue;
			});

		public static BindableProperty TextColorProperty =
			BindableProperty.Create("TextColor", typeof(Color), typeof(NavigationFrame),
			defaultValue: Color.Black,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.TextColor = (Color)newValue;
			});

		public static BindableProperty ImageProperty =
			BindableProperty.Create("Image", typeof(FileImageSource), typeof(NavigationFrame),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.Image = (FileImageSource)newValue;
			});

		public static BindableProperty BadgeValueProperty =
			BindableProperty.Create("BadgeValue", typeof(int), typeof(NavigationFrame),
			defaultValue: 0,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.BadgeValue = (int)newValue;
			});

		public static BindableProperty ImageSizeProperty =
			BindableProperty.Create("ImageSize", typeof(double), typeof(NavigationFrame),
			defaultValue: defaultImageGridHolderSize,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.ImageSize = (double)newValue;
			});

		public static BindableProperty FontSizeProperty =
			BindableProperty.Create("FontSize", typeof(double), typeof(NavigationFrame),
			defaultValue: (double)24.0,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.FontSize = (double)newValue;
			});

		public static BindableProperty CheckedProperty =
			BindableProperty.Create("Checked", typeof(bool), typeof(NavigationFrame),
			defaultValue: false,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.Checked = (bool)newValue;
			});

		public static readonly BindableProperty UncheckedAlphaProperty =
			BindableProperty.Create("UncheckedAlpha", typeof(double), typeof(NavigationFrame), 1.0);

		public static BindableProperty CommandProperty =
			BindableProperty.Create("Command", typeof(ICommand), typeof(NavigationFrame),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = ((NavigationFrame)bindable).TransparentButton;
				ctrl.Command = (ICommand)newValue;
			});

		public static BindableProperty CommandParameterProperty =
			BindableProperty.Create("CommandParameter", typeof(object), typeof(NavigationFrame),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = ((NavigationFrame)bindable).TransparentButton;
				ctrl.CommandParameter = (object)newValue;
			});

		public static BindableProperty UseHighlightProperty =
			BindableProperty.Create("UseHighlight", typeof(bool), typeof(NavigationFrame),
			defaultValue: true,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.UseHighlight = (bool)newValue;
			});

		public static BindableProperty ForcePortraitModeProperty =
			BindableProperty.Create("ForcePortraitMode", typeof(bool), typeof(NavigationFrame),
			defaultValue: false,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (NavigationFrame)bindable;
				ctrl.ForcePortraitMode = (bool)newValue;
			});

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set
			{
				SetValue(TextProperty, value);
				TextLabel.Text = value;
			}
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set
			{
				SetValue(TextColorProperty, value);
				TextLabel.TextColor = value;
			}
		}

		public double ImageSize
		{
			get { return (double)GetValue(ImageSizeProperty); }
			set
			{
				SetValue(ImageSizeProperty, value);

				imageGridHolder.HeightRequest = value;
				imageGridHolder.WidthRequest = value;
			}
		}

		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set
			{
				SetValue(FontSizeProperty, value);
				TextLabel.FontSize = value;
			}
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public FileImageSource Image
		{
			get { return (FileImageSource)GetValue(ImageProperty); }
			set
			{
				SetValue(ImageProperty, value);
				ImageImageSource.Source = value;
			}
		}

		public int BadgeValue
		{
			get { return (int)GetValue(BadgeValueProperty); }
			set
			{
				SetValue(BadgeValueProperty, value);
				BadgeImageSource.IsVisible = (value > 0)?true:false;
			}
		}

		public bool Checked
		{
			get { return (bool)GetValue(CheckedProperty); }
			set
			{
				SetValue(CheckedProperty, value);
				this.Opacity = value?1.0f:UncheckedAlpha;
			}
		}

		public double UncheckedAlpha
		{
			get { return (double)GetValue(UncheckedAlphaProperty); }
			set { SetValue(UncheckedAlphaProperty, value); }
		}

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public bool UseHighlight
		{
			get { return (bool)GetValue(UseHighlightProperty); }
			set { SetValue(UseHighlightProperty, value); }
		}

		public bool ForcePortraitMode
		{
			get { return (bool)GetValue(ForcePortraitModeProperty); }
			set { SetValue(ForcePortraitModeProperty, value); }
		}

		public void Clicked(object sender, EventArgs args)
		{
			var clicked = OnClicked;
			if (clicked != null)
				clicked(this, null);
		}
	}
}
