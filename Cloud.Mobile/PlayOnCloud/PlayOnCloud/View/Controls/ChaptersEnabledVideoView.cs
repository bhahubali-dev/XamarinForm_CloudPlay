using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public enum PlaybackExitReason
	{
		Ended,
		Error,
		Exited
	}

	public class PlaybackExitEventArgs : EventArgs
	{
		public PlaybackExitEventArgs(PlaybackExitReason reason, LibraryItem video)
		{
			this.Reason = reason;
			this.Video = video;
		}

		public PlaybackExitReason Reason { get; }
		public LibraryItem Video { get; }
	}

	public class ChaptersEnabledVideoView : View
	{
		public static readonly BindableProperty VideoProperty =
			BindableProperty.Create("Video", typeof(LibraryItem), typeof(ChaptersEnabledVideoView), null);

		public LibraryItem Video
		{
			get { return (LibraryItem) GetValue(VideoProperty); }
			set { SetValue(VideoProperty, value); }
		}

		public static readonly BindableProperty SkipAdvertisementsProperty =
			BindableProperty.Create("SkipAdvertisements", typeof(bool), typeof(ChaptersEnabledVideoView), false);

		public bool SkipAdvertisements
		{
			get { return (bool)GetValue(SkipAdvertisementsProperty); }
			set { SetValue(SkipAdvertisementsProperty, value); }
		}

		public event EventHandler<PlaybackExitEventArgs> OnPlaybackComplete;

		public void ExitPlayback(LibraryItem video, PlaybackExitReason reason)
		{
			OnPlaybackComplete?.Invoke(this, new PlaybackExitEventArgs(reason, video));
		}
	}
}
