using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IKeyboardHelper
	{
		event EventHandler<KeyboardHelperEventArgs> KeyboardChanged;

		event EventHandler<float> KeyboardShown;
	}

	public class KeyboardHelperEventArgs : EventArgs
	{
		public KeyboardHelperEventArgs(bool visible, float height, float neededOffset)
		{
			Visible = visible;
			KeyboardHeight = height;
			NeededOffset = neededOffset;
		}

		public bool Visible { get; set; }

		public float KeyboardHeight { get; set; }

		public float NeededOffset { get; set; }
	}

	public static class KeyboardHelperService
	{
		private static volatile IKeyboardHelper keyboardHelper;
		private static object syncRoot = new object();

		public static IKeyboardHelper Instance
		{
			get
			{
				if (keyboardHelper == null)
					lock (syncRoot)
						if (keyboardHelper == null)
							keyboardHelper = DependencyService.Get<IKeyboardHelper>();

				return keyboardHelper;
			}
		}

		public static event EventHandler<KeyboardHelperEventArgs> KeyboardChanged
		{
			add
			{
				Instance.KeyboardChanged += value;
			}
			remove
			{
				Instance.KeyboardChanged -= value;
			}
		}

		public static event EventHandler<float> KeyboardShown
		{
			add
			{
				Instance.KeyboardShown += value;
			}
			remove
			{
				Instance.KeyboardShown -= value;
			}
		}
	}
}
