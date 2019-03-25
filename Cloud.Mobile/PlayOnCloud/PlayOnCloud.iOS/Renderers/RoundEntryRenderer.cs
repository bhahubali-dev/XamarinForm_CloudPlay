using CoreAnimation;
using Foundation;
using PlayOnCloud;
using PlayOnCloud.iOS;
using PlayOnCloud.iOS.Controls;
using System;
using System.ComponentModel;
using System.Reflection;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RoundEntry), typeof(RoundEntryRenderer))]
namespace PlayOnCloud.iOS
{
	public class RoundEntryRenderer : ViewRenderer<Entry, CustomTextField>
	{
		private UIColor _defaultTextColor;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			CustomTextField textField = Control;
			if (Control == null)
			{
				SetNativeControl(textField = new CustomTextField());
				_defaultTextColor = textField.TextColor;
				textField.BorderStyle = UITextBorderStyle.RoundedRect;
				textField.EditingChanged += new EventHandler(OnEditingChanged);
			}

			if (e.NewElement != null)
			{
				UpdatePassword();
				UpdateText();
				UpdateColor();
				UpdateKeyboard();
				UpdateAlignment();

				var roundEntry = (RoundEntry)e.NewElement;

				SetBorder(roundEntry);
				SetFont(roundEntry);
				SetTextAlignment(roundEntry);
				SetPlaceholderTextColor(roundEntry);

				if (e.NewElement.IsPassword)
					textField.KeyboardType = UIKeyboardType.Default;
				else
					textField.KeyboardType = UIKeyboardType.Default | UIKeyboardType.EmailAddress;

				// Use 'Done' on keyboard
				textField.ReturnKeyType = !string.IsNullOrEmpty(roundEntry.NextResponderName) ? UIReturnKeyType.Next : UIReturnKeyType.Done;
				textField.EnablesReturnKeyAutomatically = true;

				// No auto-correct
				textField.AutocorrectionType = UITextAutocorrectionType.No;
				textField.SpellCheckingType = UITextSpellCheckingType.No;
				textField.AutocapitalizationType = UITextAutocapitalizationType.None;
				if (roundEntry.AutoCapitalize)
					textField.AutocapitalizationType = UITextAutocapitalizationType.Words;

				// Misc.
				textField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
				textField.ClearsOnBeginEditing = false;
				textField.ClearsOnInsertion = false;
				textField.AdjustsFontSizeToFitWidth = false;
				textField.KeyboardAppearance = UIKeyboardAppearance.Default;

				textField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				textField.Tag = roundEntry.Tag;
				textField.ResponderName = roundEntry.ResponderName;
				textField.NextResponderName = roundEntry.NextResponderName;

				textField.ShouldReturn = TextFieldShouldReturn;
			}
		}

		private bool TextFieldShouldReturn(UITextField textfield)
		{
			if (textfield is CustomTextField)
			{
				CustomTextField customTextField = textfield as CustomTextField;
				if (!string.IsNullOrEmpty(customTextField.NextResponderName))
				{
					var nextResponder = getNextResponder(UIApplication.SharedApplication.KeyWindow.RootViewController.View, customTextField.NextResponderName);
					if ((nextResponder == null) && (UIApplication.SharedApplication.KeyWindow.Subviews != null))
						foreach (var subview in UIApplication.SharedApplication.KeyWindow.Subviews)
						{
							nextResponder = getNextResponder(subview, customTextField.NextResponderName);
							if (nextResponder != null)
								break;
						}

					if (nextResponder != null)
					{
						((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, textfield.Text);
						nextResponder.BecomeFirstResponder();
						return false;
					}
				}
			}

			textfield.ResignFirstResponder();
			raiseCompleted();
			return true;
		}

		private void raiseCompleted()
		{
			var eventDelegate = (MulticastDelegate)(typeof(Entry).GetField("Completed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Element));
			if (eventDelegate != null)
				foreach (var handler in eventDelegate.GetInvocationList())
					handler.Method.Invoke(handler.Target, new object[] { Element, new EventArgs() });
		}

		private UITextField getNextResponder(UIView root, string name)
		{
			if ((root is CustomTextField) && ((root as CustomTextField).ResponderName == name))
				return root as UITextField;

			foreach (var subview in root.Subviews)
			{
				var result = getNextResponder(subview, name);
				if (result != null)
					return result;
			}

			return null;
		}

		private void SetTextAlignment(RoundEntry view)
		{
			switch (view.XAlign)
			{
				case TextAlignment.Center:
					Control.TextAlignment = UITextAlignment.Center;
					break;
				case TextAlignment.End:
					Control.TextAlignment = UITextAlignment.Right;
					break;
				case TextAlignment.Start:
					Control.TextAlignment = UITextAlignment.Left;
					break;
			}
		}

		private void SetFont(RoundEntry view)
		{
			UIFont uiFont = Font.Default.ToUIFont();
			if (uiFont != null)
				Control.Font = UIFont.FromName(uiFont.Name, (nfloat)view.FontSize);
		}

		private void SetBorder(RoundEntry view)
		{
			Control.Layer.BorderWidth = view.BorderWidth;
			Control.BorderStyle = (view.BorderWidth == 0) ? UITextBorderStyle.None : UITextBorderStyle.RoundedRect;

			Control.Layer.BorderColor = view.BorderColor.ToCGColor();
			if (view.BorderRadius > 0)
			{
				Control.Layer.CornerRadius = view.BorderRadius;
				Control.Layer.MasksToBounds = true;
				Control.Layer.SublayerTransform = CATransform3D.MakeTranslation(view.BorderRadius, 0, 0);
			}
		}

		void SetPlaceholderTextColor(RoundEntry view)
		{
			if (string.IsNullOrEmpty(view.Placeholder) == false && view.PlaceholderTextColor != Color.Default)
			{
				NSAttributedString placeholderString = new NSAttributedString(view.Placeholder, new UIStringAttributes() { ForegroundColor = view.PlaceholderTextColor.ToUIColor() });
				Control.AttributedPlaceholder = placeholderString;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdatePassword();
			else if (e.PropertyName == Entry.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Xamarin.Forms.InputView.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateColor();

			base.OnElementPropertyChanged(sender, e);
		}

		private void OnEditingChanged(object sender, EventArgs eventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, Control.Text);
		}

		private void UpdateAlignment()
		{
			if (Element.HorizontalTextAlignment == TextAlignment.Center)
				Control.TextAlignment = UITextAlignment.Center;
			else if (Element.HorizontalTextAlignment != TextAlignment.End)
				Control.TextAlignment = UITextAlignment.Left;
			else
				Control.TextAlignment = UITextAlignment.Right;
		}

		private void UpdateColor()
		{
			Color textColor = Element.TextColor;
			if (!Element.IsEnabled)
			{
				Control.TextColor = _defaultTextColor;
				return;
			}

			Control.TextColor = textColor.ToUIColor();
		}

		private void UpdateKeyboard()
		{
			Control.ApplyKeyboard(Element.Keyboard);
		}

		private void UpdatePassword()
		{
			if (Element.IsPassword && Control.IsFirstResponder)
			{
				Control.Enabled = false;
				Control.SecureTextEntry = true;
				Control.Enabled = Element.IsEnabled;
				Control.BecomeFirstResponder();
				return;
			}

			Control.SecureTextEntry = Element.IsPassword;
		}

		private void UpdateText()
		{
			if (Control.Text != Element.Text)
				Control.Text = Element.Text;
		}
	}
}
