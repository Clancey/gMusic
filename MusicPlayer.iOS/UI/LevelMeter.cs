using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using System.Text;
using UIKit;
using AVFoundation;
using MusicPlayer.Managers;
namespace MusicPlayer.iOS
{
	class LevelMeter : UIView
	{
		float[] audioLevelState;
		public float[] AudioLevelState
		{
			get { return audioLevelState; }
			set
			{
				audioLevelState = value;
				var availableHeight = Bounds.Height - 10;
				if (audioLevelState.Length < 1)
				{
					leftHeight = 0;
					rightHeight = 0;
				}
				else if (PlaybackManager.Shared.NativePlayer.CurrentTime < 1)
				{
					leftHeight = 0;
					rightHeight = 0;
				}
				else
				{
					leftHeight = audioLevelState[0] * availableHeight;
					rightHeight = audioLevelState[1] * availableHeight;
				}
				this.SetNeedsLayout();
			}
		}

		public LevelMeter() : this(CGRect.Empty)
		{

		}

		nfloat leftHeight;
		nfloat rightHeight;
		bool autoUpdate;
		UIView leftView;
		UIView rightView;
		UIView MovieView;
		public LevelMeter(CGRect rect) : base(rect)
		{
			this.BackgroundColor = UIColor.Clear;
			leftView = new UIView()
			{
				BackgroundColor = UIColor.White,
			};
			rightView = new UIView()
			{
				BackgroundColor = UIColor.White,
			};

			this.AddSubview(leftView);
			this.AddSubview(rightView);
			this.AddSubview(MovieView = new UIView());
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var padding = 5f;
			var halfPadding = padding / 2;

			var bounds = this.Bounds;
			bounds.Width /= 2;
			var mid = bounds.GetMidX();
			var width = bounds.Width / 2;

			var frame = bounds;
			frame.X = padding + width;
			frame.Width = width - padding - halfPadding;
			frame.Height = leftHeight;
			frame.Y = bounds.Height - leftHeight - padding;
			leftView.Frame = frame;

			frame.X = mid + halfPadding + width;
			frame.Height = rightHeight;
			frame.Y = bounds.Height - rightHeight - padding;
			rightView.Frame = frame;
		}
		public int MeterBars { get; set; }

		public nfloat PaddingForColumns { get; set; }

		public nfloat PaddingForBars { get; set; }

		public bool DrawEmpty { get; set; }

		public bool AutoUpdate
		{
			get { return autoUpdate; }
			set
			{
				if (value == autoUpdate)
					return;
				autoUpdate = value;
				if (autoUpdate)
					SetupNotification();
				else
					RemoveNotification();
			}
		}

		void SetupNotification()
		{
			NotificationManager.Shared.UpdateVisualizer += SharedOnUpdateVisualizer;
		}

		void SharedOnUpdateVisualizer(object sender, EventArgs eventArgs)
		{
			if (this.Superview != null)
				AudioLevelState = PlaybackManager.Shared.NativePlayer.AudioLevels;

		}

		void RemoveNotification()
		{
			try
			{
				NotificationManager.Shared.UpdateVisualizer -= SharedOnUpdateVisualizer;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}

