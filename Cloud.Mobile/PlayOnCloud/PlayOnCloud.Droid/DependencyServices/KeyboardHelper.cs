using System;
using Xamarin.Forms;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(KeyboardHelper))]
namespace PlayOnCloud.Droid
{
	class KeyboardHelper : IKeyboardHelper
	{
		public event EventHandler<KeyboardHelperEventArgs> KeyboardChanged;
		public event EventHandler<float> KeyboardShown;
	}
}