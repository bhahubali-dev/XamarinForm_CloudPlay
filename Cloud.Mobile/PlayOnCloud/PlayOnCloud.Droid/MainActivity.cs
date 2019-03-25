using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using FFImageLoading;
using FFImageLoading.Cache;
using FFImageLoading.Config;
using FFImageLoading.Forms.Droid;
using PlayOnCloud.Droid.Popups;
using Refractored.XamForms.PullToRefresh.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace PlayOnCloud.Droid
{
    [Activity(Label = "PlayOn Cloud", Icon = "@drawable/icon",
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden
    )]
    public class MainActivity : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            PopupService.Init();

            AndroidEnvironment.UnhandledExceptionRaiser += HandleAndroidException;

            Forms.Init(this, bundle);

            //XXX: Must initialize the UAS first on a main thread before anything can use it
            UserAgentService.Init();
            PullToRefreshLayoutRenderer.Init();
            CachedImageRenderer.Init();
            ImageService.Instance.Initialize(new Configuration
            {
                HttpClient = RestService.Instance.Client,
                FadeAnimationEnabled = false,
                FadeAnimationForCachedImages = false,
                BitmapOptimizations = false,
                TransformPlaceholders = false
            });

            var downloadCache = ImageService.Instance.Config.DownloadCache as DownloadCache;
            if (downloadCache != null)
                downloadCache.DelayBetweenRetry = TimeSpan.FromSeconds(0);
            else
                Logger.Log("WARNING: Unable to cast FFImageLoading DownloadCache!");


            var app = new App();
            LoadApplication(app);
            app.Activate();
            if ((int) Build.VERSION.SdkInt >= 21)
                ActionBar.SetIcon(new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }

        private void HandleAndroidException(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            Console.Write("HANDLED EXCEPTION");
        }

        public override void OnBackPressed()
        {
            BackButtonHelper.FireBackButtonPressed();
        }
    }
}