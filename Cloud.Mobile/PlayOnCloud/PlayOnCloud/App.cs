using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PlayOnCloud
{
	public partial class App : Application
	{
		private static bool keyboardTrackingEnabled = true;

		public static bool KeyboardTrackingEnabled
		{
			get { return keyboardTrackingEnabled; }
			set { keyboardTrackingEnabled = value; }
		}

		private ViewModel.Cloud viewModel;
		private bool sleeping = false;
		private TimeSpan pollInterval = TimeSpan.FromSeconds(5);
		private TaskQueue pollQueue = new TaskQueue();

		public App()
		{
			InitializeComponent();

			//XXX: Fix xamarin linking bug
			new FFImageLoading.Transformations.CircleTransformation();

			//Set view model bindings
			viewModel = new ViewModel.Cloud();
			this.BindingContext = viewModel;

			//Load UI
			var postLoad = new PostLoad() { BindingContext = viewModel };
			MainPage = new NavigationPage(postLoad)
			{
				BarTextColor = Color.White,
				BarBackgroundColor = Color.FromHex("#303030")
			};

			//Keyboard shown notification
			KeyboardHelperService.KeyboardChanged += KeyboardHelper_KeyboardChanged;
		}

		public async Task<bool> PerformFetch()
		{
			var result = false;

			try
			{
				if (viewModel != null)
				{
					if (viewModel.Library.AutoDownload)
					{
						LoggerService.Instance.Log("App.PerformFetch: Auto download is ON");
						if (!viewModel.Account.SignedIn)
						{
							LoggerService.Instance.Log("App.PerformFetch: Account not logged in. Trying to log in.");

							var stored = viewModel.Account.LoadUserFromCredentials();
							if (stored == null)
							{
								LoggerService.Instance.Log("App.PerformFetch: Account stored credits are null.");
								return result;
							}

							if (string.IsNullOrEmpty(stored.AuthToken))
							{
								LoggerService.Instance.Log("App.PerformFetch: Account AuthToken is null.");
								return result;
							}

							var error = await viewModel.Account.SignInWithTokenAsync(stored, false);
							if (!string.IsNullOrEmpty(error))
								LoggerService.Instance.Log("ERROR: App.PerformFetch: SignIn Error: " + error);

							LoggerService.Instance.Log("App.PerformFetch: SignIn success");
						}

						if (!viewModel.Library.Initialized)
							await viewModel.Library.Init();
						else
							await viewModel.Library.Poll(false);

						result = true;
					}
					else
					{
						LoggerService.Instance.Log("App.PerformFetch: Auto download is OFF");
						result = false;
					}
				}
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: App.PerformFetch: " + ex);
			}

			return result;
		}

		public void Activate()
		{
			LoggerService.Instance.Log("App.Activate");

			Device.StartTimer(pollInterval, PollTimerCallBack);
			MarkManager.StartMarkTimer();

			viewModel?.SetIsInBackground(false);

			var postLoad = (MainPage as NavigationPage)?.CurrentPage as PostLoad;
			if ((postLoad != null) && !postLoad.InitiateSignInSequencePerformed)
			{
				var task = postLoad.InitiateSignInSequence();
			}
		}

		protected override void OnStart()
		{
			LoggerService.Instance.Log("Current version: " + AppVersionNumberService.Instance.GetVersion());
			FacebookToolsService.Instance.ActivateApp();
		}

		protected override void OnSleep()
		{
			LoggerService.Instance.Log("App: OnSleep");

			sleeping = true;
			viewModel?.SetIsInBackground(true);

			MarkManager.StopMarkTimer();
			base.OnSleep();
		}

		protected override async void OnResume()
		{
			LoggerService.Instance.Log("App: OnResume");

			sleeping = false;
			if (viewModel != null)
			{
				viewModel.SetIsInBackground(false);
				if (!pollQueue.Running)
					await pollQueue.EnqueueAsync(async () => await viewModel.Poll(true));
				else
					await pollQueue.WaitAllTasks();
			}

			FacebookToolsService.Instance.ActivateApp();
			base.OnResume();
		}

		private bool PollTimerCallBack()
		{
			if (sleeping)
				return false;

			if (!pollQueue.Running)
				pollQueue.Enqueue(() => viewModel?.Poll(false));

			return true;
		}

		private void KeyboardHelper_KeyboardChanged(object sender, KeyboardHelperEventArgs e)
		{
			//Shift page up/down wnen keyboard is shown;
			if (App.KeyboardTrackingEnabled)
			{
				if (e.Visible)
					MainPage.TranslationY -= e.NeededOffset;
				else
					MainPage.TranslationY = 0;
			}
		}
	}
}
