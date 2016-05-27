using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Models;
using UIKit;
using SimpleTables;
using CoreGraphics;
using MusicPlayer.iOS;

namespace MusicPlayer
{
	internal class AlbumSongCell : ICell
	{
		readonly Song song;

		public AlbumSongCell(Song song)
		{
			this.song = song;
		}

		public UITableViewCell GetCell(UITableView tableview)
		{
			Cell cell = tableview.DequeueReusableCell(Cell.Key) as Cell ?? new Cell();
			cell.Update(song);
			cell.ApplyStyle(tableview);
			return cell;
		}

		public class Cell : MediaItemCell
		{
			public const string Key = "artistSongCell";
			UILabel trackLabel;

			public Cell() : base(Key,false)
			{
				trackLabel = new UILabel
				{
					TextAlignment = UITextAlignment.Center,
					Text = "Test",
				}.StyleAsMainText();
				trackLabel.SizeToFit();
				this.ContentView.AddSubview(trackLabel);
            }
			

			public override void TappedAccessory(SimpleButton button)
			{
				PopupManager.Shared.Show(BindingContext, button);
			}
			WeakReference bindingContext;

			public Song BindingContext
			{
				get { return bindingContext?.Target as Song; }
				set
				{
					if (value == null)
					{
						bindingContext = null;
						return;
					}
					bindingContext = new WeakReference(value);
				}
			}

			public async void Update(Song song)
			{
				BindingContext = song;
				TextView.TopLabel.Text = song?.Name ?? "";
				TextView.BottomLabel.Text = song?.Artist ?? "";
				trackLabel.Text = song?.Track.ToString() ?? "";
				TextView.SetNeedsLayout();

				ShowOffline = song.OfflineCount > 0;
				MediaTypeImage.Hidden = !song.HasVideo;
			}

			const float accessoryWidth = 30f;
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				const float SideMargin = 5f;
				const float TrackWidth = 30f;
				const float Padding = 4.5f;


				var bounds = ContentView.Frame;
				var frame = bounds;
				var midY = bounds.Bottom/2;
				frame.X += SideMargin;
				frame.Width = TrackWidth;
				frame.Height = trackLabel.Frame.Height;
				frame.Y = midY - frame.Height/2;
				trackLabel.Frame = frame;
				trackLabel.Center = ImageView.Center;
			}
		}
	}
}