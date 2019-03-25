using PlayOnCloud.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class MediaPlayer : ContentPage
	{
		public MediaPlayer()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		public static readonly BindableProperty SkipAdvertisementsProperty =
			BindableProperty.Create(
				"SkipAdvertisements",
				typeof(bool),
				typeof(MediaPlayer),
				false,
				BindingMode.Default,
				null,
				propertyChanging: (bindable, oldValue, newValue) =>
				{
					var mediaPlayer = (MediaPlayer)bindable;
					mediaPlayer.SkipAdvertisements = (bool)newValue;
					mediaPlayer.chaptersEnabledVideoView.SkipAdvertisements = (bool)newValue;
				});

		public bool SkipAdvertisements
		{
			get { return (bool)GetValue(SkipAdvertisementsProperty); }
			set { SetValue(SkipAdvertisementsProperty, value); }
		}

		public static readonly BindableProperty VideoProperty =
			BindableProperty.Create(
				"Video",
				typeof(LibraryItem),
				typeof(MediaPlayer),
				null,
				BindingMode.Default,
				null,
				propertyChanging: (bindable, oldValue, newValue) =>
				{
					var mediaPlayer = (MediaPlayer)bindable;
					mediaPlayer.Video = (LibraryItem)newValue;
					mediaPlayer.chaptersEnabledVideoView.Video = (LibraryItem)newValue;
				});

		public LibraryItem Video
		{
			get { return (LibraryItem)GetValue(VideoProperty); }
			set { SetValue(VideoProperty, value); }
		}

		public event EventHandler<PlaybackExitEventArgs> OnMediaPlayerFinished;

		private async void ChaptersEnabledVideoView_OnOnPlaybackComplete(object sender, PlaybackExitEventArgs args)
		{
			if (args.Reason == PlaybackExitReason.Error)
				await DisplayAlert("Playback Error", "There was an error playing the selected video.", "OK");

			OnMediaPlayerFinished?.Invoke(this, args);
		}
	}
}
