using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using UIKit;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	internal class ProgressView : UIView
	{
		CustomProgress downloadProgess;
		CustomProgress sliderProgress;
		OBSlider slider;

		public Action EditingStarted {get;set;}
		public Action EditingEnded {get;set;}

		public ProgressView()
		{
			downloadProgess = new CustomProgress()
			{
				ProgressTintColor = UIColor.DarkGray,
				BackgroundColor = UIColor.White,
			};
			downloadProgess.SizeToFit();
			var style = this.GetStyle();
			sliderProgress = new CustomProgress()
			{
				ProgressTintColor = style.AccentColorHorizontal,
			};
			slider = new OBSlider();
			slider.AccessibilityIdentifier = "Progress";
			slider.BackgroundColor = UIColor.Clear;
			slider.MinimumTrackTintColor = style.AccentColorHorizontal;
			slider.MaximumTrackTintColor = UIColor.Clear;
			slider.SizeToFit();
			slider.ValueChanged += (object sender, EventArgs e) => { sliderProgress.Progress = slider.Value; };
			slider.EditingDidBegin += (object sender, EventArgs e) =>  {EditingStarted?.Invoke();};
			slider.EditingDidEnd += (sender, args) => {
				EditingEnded?.Invoke();
				PlaybackManager.Shared.Seek(slider.Value);
			};
			slider.SetThumbImage(Images.GetPlaybackSliderThumb(), UIControlState.Normal);
			this.Frame = slider.Frame;
			this.AddSubview(downloadProgess);
			this.AddSubview(sliderProgress);
			this.AddSubview(slider);
		}

		public void SetAlpha(nfloat alpha)
		{
			alpha = NMath.Max(0, alpha);
			alpha = NMath.Min(1, alpha);
			slider.Alpha = alpha;
			sliderProgress.Alpha = 1f - alpha;
		}

		public nfloat VisibleHeight
		{
			get { return downloadProgess.Frame.Height; }
		}

		public override void LayoutSubviews()
		{
			var frame = Bounds;
			frame.Width += 40;

			slider.Frame = frame;

			frame = downloadProgess.Frame;
			//frame.X = 5;
			frame.Width = Bounds.Width + 5;
			sliderProgress.Frame = downloadProgess.Frame = frame;
			sliderProgress.Center =
				slider.Center = downloadProgess.Center = new CoreGraphics.CGPoint(Bounds.GetMidX(), Bounds.GetMidY());
		}

		public float DownloadProgress
		{
			get { return downloadProgess.Progress; }
			set { downloadProgess.Progress = value; }
		}

		public float SliderProgress
		{
			get { return slider.Value; }
			set
			{
				if (slider.Tracking)
					return;
				slider.Position = sliderProgress.Progress = value;
			}
		}

		class CustomProgress : UIView
		{
			UIView progressView;
			UIImage progressImage;

			public UIImage ProgressImage
			{
				get { return progressImage; }
				set
				{
					progressImage = value;
					progressView.BackgroundColor = value == null ? UIColor.Clear : UIColor.FromPatternImage(value);
				}
			}

			public UIColor TrackTintColor
			{
				get { return BackgroundColor; }
				set { BackgroundColor = value; }
			}

			public UIColor ProgressTintColor
			{
				get { return progressView.BackgroundColor; }
				set { progressView.BackgroundColor = value; }
			}

			float progress;

			public float Progress
			{
				get { return progress; }
				set
				{
					progress = value;
					this.SetNeedsLayout();
				}
			}

			public CustomProgress() : base(new CGRect(0, 0, 100, 2.5f))
			{
				Add(progressView = new UIView());
				this.ClipsToBounds = true;
			}

			public override void LayoutSubviews()
			{
				var frame = Bounds;
				frame.Width *= NMath.Max(progress, 0);
				progressView.Frame = frame;
			}
		}
	}
}