using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using SDWebImage;
using MusicPlayer.iOS.Controls;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using RadialProgress;
using UIKit;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{
	class SongDownloadCell : BaseCell
	{
		#region implemented abstract members of BaseCell

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
			public const string Key = "SongDownloadCell";
			RadialProgress.RadialProgressView progress;
			public SongTableViewCell() : base(Key)
			{
				var style = this.GetStyle();
				Add(progress = new RadialProgress.RadialProgressView(progressType:RadialProgressViewStyle.Small)
				{
					ProgressColor = style.AccentColor
				});
				NotificationManager.Shared.SongDownloadPulsed+= (sender, args) =>
				{
					if (BindingContext?.Id != args.SongId)
						return;
					progress.Hidden = false;
					progress.Value = args.Percent;
				};
				ImageView.Layer.BorderColor = UIColor.Clear.CGColor;
				AccessoryView = null;
			}

			WeakReference bindingContext;

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				progress.Center = ImageView.Center;
			}

			public Song BindingContext
			{
				get { return bindingContext?.Target as Song ?? null; }
				set
				{
					if (BindingContext == value)
						return;
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
			}

			void SetValues(Song song)
			{
				if (song == null)
					return;
				ShowOffline = song.OfflineCount > 0;
				SetText(song);
				progress.Value = 0;
				progress.Hidden = true;

			}
		}
	}
}