using System;
using System.Collections.Generic;
using System.IO;
using CoreGraphics;
using Foundation;
using MediaPlayer;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using PlayOnCloud.Model;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(ChaptersEnabledVideoView), typeof(ChaptersEnabledVideoViewRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class ChaptersEnabledVideoViewRenderer : ViewRenderer<ChaptersEnabledVideoView, UIChaptersEnabledVideoView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ChaptersEnabledVideoView> e)
		{
			base.OnElementChanged(e);

			Logger.Log("ChaptersEnabledVideoViewRenderer.OnElementChanged: e.NewElement='" + e.NewElement + "', Control='" + Control + "'");

			if ((e.NewElement != null) && (Control == null))
				SetNativeControl(new UIChaptersEnabledVideoView(e.NewElement.Video, UIScreen.MainScreen.Bounds, e.NewElement));
		}
	}

	public sealed class UIChaptersEnabledVideoView : UIView
	{
		private MPMoviePlayerViewController moviePlayer;
		private ChaptersEnabledVideoView videoView;
		private LibraryItem video;

		private NSTimer adSkipTimer;
		private TimeSpan initialPlaybackTime = TimeSpan.Zero;
		private bool initialPlaybackTimeSet;

		private NSObject playbackDidFinishNotification;
		private NSObject playbackIsPreparedToPlayNotification;
		private NSObject playbackStateDidChangeNotification;
		private NSObject willEnterForegroundNotification;
		private NSObject didEnterBackgroundNotification;

		protected override void Dispose(bool disposing)
		{
			Logger.Log("UIChaptersEnabledVideoView.Dispose: disposing = " + disposing);

			if (disposing)
			{
				willEnterForegroundNotification?.Dispose();
				willEnterForegroundNotification = null;

				didEnterBackgroundNotification?.Dispose();
				didEnterBackgroundNotification = null;

				playbackDidFinishNotification?.Dispose();
				playbackDidFinishNotification = null;

				playbackIsPreparedToPlayNotification?.Dispose();
				playbackIsPreparedToPlayNotification = null;

				playbackStateDidChangeNotification?.Dispose();
				playbackStateDidChangeNotification = null;

				adSkipTimer?.Dispose();
				adSkipTimer = null;

				if (moviePlayer != null)
				{
					moviePlayer.MoviePlayer?.Dispose();
					moviePlayer.Dispose();
					moviePlayer = null;
				}
			}

			base.Dispose(disposing);
		}

		public UIChaptersEnabledVideoView(LibraryItem video, CGRect frame, ChaptersEnabledVideoView videoView)
		{
			this.videoView = videoView;
			this.video = video;

			AutoresizingMask = UIViewAutoresizing.All;
			ContentMode = UIViewContentMode.ScaleAspectFill;

			if (!string.IsNullOrEmpty(video.LocalFilePath))
			{
				Logger.Log("UIChaptersEnabledVideoView.ctor: video.LocalFilePath = '" + video.LocalFilePath + "'");

				MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = new MPNowPlayingInfo()
				{
					Title = video.Title
				};

				var linkPath = LocalLibraryService.Instance.GetLinkForMediaItem(video);
				moviePlayer = new MPMoviePlayerViewController(NSUrl.FromFilename(string.IsNullOrEmpty(linkPath) ? video.LocalFilePath : linkPath))
				{
					View =
					{
						ContentMode = UIViewContentMode.ScaleAspectFill,
						AutoresizingMask = UIViewAutoresizing.All
					},

					MoviePlayer =
					{
						RepeatMode = MPMovieRepeatMode.None,
						ControlStyle = MPMovieControlStyle.Fullscreen,
						ScalingMode = MPMovieScalingMode.AspectFit,
						AllowsAirPlay = true
					}
				};

				Frame = moviePlayer.View.Frame = frame;
				Add(moviePlayer.View);

				playbackDidFinishNotification = MPMoviePlayerController.Notifications.ObservePlaybackDidFinish(playBackFinishedHandler);

				NSNotificationCenter.DefaultCenter.RemoveObserver(moviePlayer, UIApplication.DidEnterBackgroundNotification, null);
				willEnterForegroundNotification = UIApplication.Notifications.ObserveWillEnterForeground(willEnterForegroundHandler);
				didEnterBackgroundNotification = UIApplication.Notifications.ObserveDidEnterBackground(didEnterBackgroundHandler);

				var bookmarkTime = (int)video.BookmarkTime;
				if ((bookmarkTime == -2) || (bookmarkTime == 0))
					bookmarkTime = -1;

				if (videoView.SkipAdvertisements)
				{
					bool hasBookmarkTimeValue = false;

					for (int i = 0; i < video.Chapters.Count; i++)
					{
						Chapter chapter = video.Chapters[i];
						chapter.IsSkipped = false;

						if (!hasBookmarkTimeValue && (chapter.Type != ChapterType.Advertisement))
						{
							int chapterStartTime = (int)chapter.StartTime;
							if (chapterStartTime != 0)
								bookmarkTime = chapterStartTime;

							hasBookmarkTimeValue = true;
						}
					}
				}

				if ((videoView.SkipAdvertisements) || ((bookmarkTime > 0) && UIDevice.CurrentDevice.CheckSystemVersion(8, 4)))
					playbackStateDidChangeNotification = MPMoviePlayerController.Notifications.ObservePlaybackStateDidChange(playbackStateDidChange);

				if (bookmarkTime > 0)
				{
					if (UIDevice.CurrentDevice.CheckSystemVersion(8, 4))
						initialPlaybackTime = TimeSpan.FromSeconds(bookmarkTime);
					else
						moviePlayer.MoviePlayer.InitialPlaybackTime = bookmarkTime;
				}

				if (moviePlayer.MoviePlayer.IsPreparedToPlay)
					moviePlayer.MoviePlayer.Play();
				else
				{
					playbackIsPreparedToPlayNotification = MPMoviePlayerController.Notifications.ObserveMediaPlaybackIsPreparedToPlayDidChange(playBackIsPreparedToPlayHandler);
					moviePlayer.MoviePlayer.PrepareToPlay();
				}
			}
			else
				exitPlayback(PlaybackExitReason.Error);
		}

		private void playbackStateDidChange(object sender, NSNotificationEventArgs nsNotificationEventArgs)
		{
			if ((adSkipTimer != null) && videoView.SkipAdvertisements)
			{
				adSkipTimer.Dispose();
				adSkipTimer = null;
			}

			if (moviePlayer.MoviePlayer.PlaybackState == MPMoviePlaybackState.Playing)
			{
				if (!initialPlaybackTimeSet && (initialPlaybackTime != TimeSpan.Zero))
				{
					moviePlayer.MoviePlayer.CurrentPlaybackTime = initialPlaybackTime.TotalSeconds;
					initialPlaybackTimeSet = true;
				}

				if (videoView.SkipAdvertisements)
					armAdSkipTimer();
			}
		}

		private void armAdSkipTimer()
		{
			double currentPlaybackTime = moviePlayer.MoviePlayer.CurrentPlaybackTime;
			if (currentPlaybackTime < 0)
				currentPlaybackTime = 0;

			for (int i = 0; i < video.Chapters.Count; i++)
			{
				Chapter advertisementChapter = video.Chapters[i];
				if ((advertisementChapter.Type == ChapterType.Advertisement) && !advertisementChapter.IsSkipped)
				{
					TimeSpan adSkipNextVideoStartTime = TimeSpan.FromSeconds(moviePlayer.MoviePlayer.Duration);
					for (int j = (i + 1); j < video.Chapters.Count; j++)
					{
						Chapter videoChapter = video.Chapters[j];
						if (videoChapter.Type != ChapterType.Advertisement)
						{
							adSkipNextVideoStartTime = TimeSpan.FromSeconds(videoChapter.StartTime);
							break;
						}
					}

					bool isCurrentTimeBeforeAd = (advertisementChapter.StartTime > currentPlaybackTime);
					bool isCurrentTimeOverAd = ((currentPlaybackTime > advertisementChapter.StartTime) && (currentPlaybackTime < adSkipNextVideoStartTime.TotalSeconds));

					if (isCurrentTimeOverAd)
					{
						seekMoviePlayer(advertisementChapter, (int)adSkipNextVideoStartTime.TotalSeconds);
						break;
					}
					else if (isCurrentTimeBeforeAd)
					{
						TimeSpan timeUntilAdvertisement = TimeSpan.FromSeconds(advertisementChapter.StartTime - currentPlaybackTime);
						adSkipTimer = NSTimer.CreateScheduledTimer(timeUntilAdvertisement, act =>
						{
							seekMoviePlayer(advertisementChapter, (int)adSkipNextVideoStartTime.TotalSeconds);
						});

						break;
					}
				}
			}
		}

		private void seekMoviePlayer(Chapter advertisementChapter, int nextPlaybackTime)
		{
			advertisementChapter.IsSkipped = true;
			moviePlayer.MoviePlayer.CurrentPlaybackTime = nextPlaybackTime;
		}

		private void playBackIsPreparedToPlayHandler(object sender, NSNotificationEventArgs args)
		{
			Logger.Log("UIChaptersEnabledVideoView.playBackIsPreparedToPlayHandler:" +
				"PlaybackState = '" + moviePlayer.MoviePlayer.PlaybackState + "', LoadState='" + moviePlayer.MoviePlayer.LoadState + "'");

			moviePlayer.MoviePlayer.Play();
		}

		private void playBackFinishedHandler(object sender, MPMoviePlayerFinishedEventArgs mpMoviePlayerFinishedEventArgs)
		{
			PlaybackExitReason reason = (PlaybackExitReason)mpMoviePlayerFinishedEventArgs.FinishReason;
			if (reason == PlaybackExitReason.Ended)
				video.BookmarkTime = -2;
			else
				video.BookmarkTime = moviePlayer.MoviePlayer.CurrentPlaybackTime;

			exitPlayback(reason);
		}

		private void willEnterForegroundHandler(object sender, NSNotificationEventArgs args)
		{
			Logger.Log("UIChaptersEnabledVideoView.willEnterForegroundHandler");

			if (moviePlayer.MoviePlayer.PlaybackState != MPMoviePlaybackState.Playing)
				moviePlayer.MoviePlayer.Play();
		}

		private void didEnterBackgroundHandler(object sender, NSNotificationEventArgs args)
		{
			Logger.Log("UIChaptersEnabledVideoView.didEnterBackgroundHandler");

			if(!moviePlayer.MoviePlayer.AirPlayVideoActive)
				moviePlayer.MoviePlayer.Pause();
		}

		private void exitPlayback(PlaybackExitReason reason)
		{
			Logger.Log("UIChaptersEnabledVideoView.exitPlayback: reason = " + reason);

			var vv = videoView;
			if (vv != null)
				Device.BeginInvokeOnMainThread(() =>
				{
					//XXX: Giant hack to force us back to portrait mode on phones upon exiting the video player that might have been in landscape
					//alternatives - https://forums.xamarin.com/discussion/comment/142012/#Comment_142012 - https://forums.xamarin.com/discussion/42849/fix-orientation-for-one-page-in-the-app-is-it-possible-with-xamarin-forms
					if (Device.Idiom == TargetIdiom.Phone)
						UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));

					vv.ExitPlayback(video, reason);

					if (Device.Idiom == TargetIdiom.Phone)
						UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Unknown), new NSString("orientation"));
				});
		}
	}
}
