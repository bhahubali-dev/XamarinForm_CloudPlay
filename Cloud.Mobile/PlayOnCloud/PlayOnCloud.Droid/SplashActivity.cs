using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace PlayOnCloud.Droid
{
    [Activity(Label = "PlayOn Cloud", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }
    }
}