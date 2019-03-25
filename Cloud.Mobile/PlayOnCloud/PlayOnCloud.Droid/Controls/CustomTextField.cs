using Android.Runtime;
namespace PlayOnCloud.Droid.Controls
{
    [Register("CustomTextField")]
    public class CustomTextField : Xamarin.Forms.Entry
    {
        public string ResponderName { get; set; }

        public string NextResponderName { get; set; }
    }
}