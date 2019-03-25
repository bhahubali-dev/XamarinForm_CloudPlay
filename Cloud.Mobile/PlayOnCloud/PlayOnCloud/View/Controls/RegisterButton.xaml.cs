using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class RegisterButton : ContentView
	{
		public event EventHandler OnClicked;

		public RegisterButton()
		{
			InitializeComponent();
		}

		private void roundButtonClicked(object sender, EventArgs e)
		{
			var clicked = OnClicked;
			if (clicked != null)
				clicked(sender, e);
		}

		public static BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(RegisterButton),
			defaultValue: string.Empty,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.textLabel.Text = (string)newValue;
			});

		public static BindableProperty MarginTextProperty =
			BindableProperty.Create(nameof(MarginText), typeof(Thickness), typeof(RegisterButton),
			defaultValue: new Thickness(0, 0, 0, 0),
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.textLabel.Margin = (Thickness)newValue;
			});

		public static BindableProperty BoldTextProperty =
			BindableProperty.Create(nameof(BoldText), typeof(string), typeof(RegisterButton),
			defaultValue: string.Empty,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.textLabel.BoldText = (string)newValue;
			});

		public static BindableProperty TextColorProperty =
			BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(RegisterButton),
			defaultValue: Color.Black,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.textLabel.TextColor = (Color)newValue;
			});

		public static BindableProperty BackgroundProperty =
			BindableProperty.Create(nameof(Background), typeof(Color), typeof(RegisterButton),
			defaultValue: Color.Black,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.roundButton.BackgroundColor = (Color)newValue;
			});

		public static BindableProperty ImageSourceTextProperty =
			BindableProperty.Create(nameof(ImageSourceText), typeof(string), typeof(RegisterButton),
			defaultValue: string.Empty,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.imageSource.Source = (string)newValue;
			});

		public static BindableProperty ButtonCommandProperty =
			BindableProperty.Create(nameof(ButtonCommand), typeof(Command), typeof(RegisterButton),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.roundButton.Command = (Command)newValue;
			});

		public static BindableProperty ButtonCommandParameterProperty =
			BindableProperty.Create(nameof(ButtonCommandParameter), typeof(object), typeof(RegisterButton),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanging: (bindable, oldValue, newValue) =>
			{
				var ctrl = (RegisterButton)bindable;
				ctrl.roundButton.CommandParameter = (object)newValue;
			});

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set
			{
				SetValue(TextProperty, value);
				textLabel.Text = value;
			}
		}

		public Thickness MarginText
		{
			get { return (Thickness)GetValue(MarginTextProperty); }
			set
			{
				SetValue(MarginTextProperty, value);
				textLabel.Margin = value;
			}
		}

		public string BoldText
		{
			get { return (string)GetValue(BoldTextProperty); }
			set
			{
				SetValue(BoldTextProperty, value);
				textLabel.BoldText = value;
			}
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set
			{
				SetValue(TextColorProperty, value);
				textLabel.TextColor = value;
			}
		}

		public Color Background
		{
			get { return (Color)GetValue(BackgroundProperty); }
			set
			{
				SetValue(BackgroundProperty, value);
				roundButton.BackgroundColor = value;
			}
		}

		public string ImageSourceText
		{
			get { return (string)GetValue(ImageSourceTextProperty); }
			set
			{
				SetValue(ImageSourceTextProperty, value);
				imageSource.Source = value;
			}
		}

		public Command ButtonCommand
		{
			get { return (Command)GetValue(ButtonCommandProperty); }
			set
			{
				SetValue(ButtonCommandProperty, value);
				roundButton.Command = value;
			}
		}

		public object ButtonCommandParameter
		{
			get { return (object)GetValue(ButtonCommandParameterProperty); }
			set
			{
				SetValue(ButtonCommandParameterProperty, value);
				roundButton.CommandParameter = value;
			}
		}
	}
}
