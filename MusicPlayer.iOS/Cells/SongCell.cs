using System;
using CoreGraphics;
using UIKit;
using MusicPlayer.Models;
using Foundation;
using SDWebImage;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using MusicPlayer.Data;

namespace MusicPlayer.iOS
{
	public class SongCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		static LevelMeter meter;
		public bool IncludeVisualizer { get; set; }
		static LevelMeter Meter
		{
			get { return meter ?? (meter = new LevelMeter(new CGRect(0, 0, 5, 5))); }
			set
			{
				meter = value;
			}
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(SongTableViewCell.Key) as SongTableViewCell ?? new SongTableViewCell();
			cell.BindingContext = BindingContext as Song;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		public class SongTableViewCell : MediaItemCell
		{
			public const string Key = "SongCell";
			static UIView overlay = new UIView(new CGRect(0,0,MediaItemCell.ImageWidth,MediaItemCell.ImageWidth))
			{
				Alpha = .5f,
			};
			public SongTableViewCell() : base(Key)
			{
				
			}
			public override void ApplyStyle(UITableView tv)
			{
				base.ApplyStyle(tv);

				var style = this.GetStyle();
				overlay.BackgroundColor = style.BackgroundColor;
			}

			WeakReference bindingContext;

			public Song BindingContext
			{
				get { return bindingContext?.Target as Song; }
				set
				{
					if (BindingContext != null)
						ClearEvents();
					if (value == null)
					{
						bindingContext = null;
						return;
					}
					bindingContext = new WeakReference(value);
					SetValues(value);
				}
			}

			public override void TappedAccessory(SimpleButton button)
			{
				PopupManager.Shared.Show(BindingContext, button);
			}

			void ClearEvents()
			{
				var song = BindingContext;
				if (song == null)
					return;
				if (Meter.Superview == ImageView)
				{
					Meter.RemoveFromSuperview();
					overlay.RemoveFromSuperview();
				}
			}


			async void SetValues(Song song)
			{
				if (song == null)
					return;
				ShowOffline = song.OfflineCount > 0;
				MediaTypeImage.Hidden = !song.HasVideo;
				SetText(song);
				var locaImage = await song.GetLocalImage(ImageWidth);
				if (locaImage != null)
				{
					ImageView.Image = locaImage;
				}
				else
				{
					var artUrl = await ArtworkManager.Shared.GetArtwork(song);
					if (string.IsNullOrWhiteSpace(artUrl))
						ImageView.Image = Images.GetDefaultSongImage(ImageWidth);
					else
						ImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultSongImage(ImageWidth));
				}

				if (song != null && song.Id == Settings.CurrentSong)
				{
					ImageView.AddSubview(overlay);
					ImageView.AddSubview(Meter);
					Meter.AutoUpdate = true;
					Meter.Frame = ImageView.Bounds.Inset(5, 5);
				}
				else if (Meter.Superview == ImageView)
				{
					Meter.RemoveFromSuperview();
					overlay.RemoveFromSuperview();
				}
			}
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				if (Meter.Superview == ImageView)
				{
					Meter.Frame = ImageView.Bounds.Inset(5,5);
					overlay.Frame = ImageView.Bounds;
				}
			}
		}
	}
}