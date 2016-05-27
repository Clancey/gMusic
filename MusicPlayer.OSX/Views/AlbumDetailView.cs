using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using MusicPlayer.Managers;
using MusicPlayer.Data;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public partial class AlbumDetailView : AppKit.NSView , INavigationItem
	{
		#region Constructors

		// Called when created from unmanaged code
		public AlbumDetailView (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public AlbumDetailView (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		#region INavigationItem implementation

		public string Title {
			get {
				return Album?.Name;
			}
		}

		public NSNavigationController NavigationController {get;set;}

		#endregion

		WeakReference _album;

		public Album Album {
			get {
				return _album?.Target as Album;
			}
			set {
				_album = new WeakReference (value);
				Update (value);
			}
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		bool isSetup;

		public override void ViewDidMoveToSuperview ()
		{
			base.ViewDidMoveToSuperview ();
			if (isSetup)
				return;
			isSetup = true;

			PlayButton.Image = Images.GetPlaybackButton (15);
			PlayButton.Activated += (object sender, EventArgs e) => {
				PlaybackManager.Shared.Play (Album);
			};
			ShuffleButton.Image = Images.GetShuffleImage (15);
			ShuffleButton.Activated += (object sender, EventArgs e) => {
				Settings.ShuffleSongs = true;
				PlaybackManager.Shared.Play (Album);
			};
			MoreButton.Image = Images.DisclosureImage.Value;
			MoreButton.Hidden = true;
		}

		async void Update (Album album)
		{
			LineView.BackgroundColor = NSColor.SecondaryLabelColor;
			TitleLabel.StringValue = album?.Name ?? "";
			var detailString = album?.Artist ?? "";
			if (album?.Year > 0)
				detailString = $"{album.Artist} • {album.Year}";

			DetailLabel.StringValue = detailString;
			TableView.Source = new AlbumDetailsTwinListViewModel{ Album = album, AutoPlayOnSelect = false };
			this.ResizeSubviewsWithOldSize (Bounds.Size);
			await Task.WhenAll (ImageView.LoadFromItem (album),
				BackgroundImageView.LoadFromItem (album, (float)ImageView.Bounds.Width));

		}

		const float BaseHeight = 160;
		const float MinHeight = 300;
		public float GetHeight ()
		{
			var rows = TableView.Source.GetRowCount (TableView);
			var tableHeight = rows * 46;
			return Math.Min (MinHeight, tableHeight + BaseHeight);

		}

		const float minLeft = 350;
		const float padding = 10f;

		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			TitleLabel.SizeToFit ();
			var titleFrame = TitleLabel.Frame;
			var frame = PlayButton.Frame;
			frame.Y = titleFrame.Y - 3;
			frame.X = titleFrame.Right + padding;
			PlayButton.Frame = frame;

			frame.X += frame.Width + padding;
			ShuffleButton.Frame = frame;
			frame.X += frame.Width + padding;
			MoreButton.Frame = frame;
		}

		public class AlbumDetailsTwinListViewModel : AlbumDetailsViewModel
		{
			int CurrentRowCount;
			const int MinRows = 5;

			public override nint GetRowCount (NSTableView tableView)
			{
				var sectionCount = NumberOfSections ();
				SetTable (tableView);
				if (sectionRows.Count == 0)
					ReloadData ();
				var rowCount = sectionRows.LastOrDefault ()?.IndexEnd + 1 ?? 0;
				if (rowCount < MinRows)
					return CurrentRowCount = rowCount;
				CurrentRowCount = (int)((double)rowCount / 2);
				return CurrentRowCount;
			}

			public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
			{
				var song = GetSong (tableColumn, row);
				var cell = tableView.MakeView (AlbumDetailsSongCell.Key, this) as AlbumDetailsSongCell ?? new AlbumDetailsSongCell ();
				cell.ViewModel = this;
				cell.Song = song;
				return cell;
			}

			public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, nint row)
			{
				var song = GetSong (tableColumn, row);
				return (NSString)(song?.ToString () ?? "");
			}

			Song GetSong (NSTableColumn tableColumn, nint row)
			{
				Song song = null;
				if (CurrentRowCount < MinRows) {
					if (tableColumn.Identifier == "2")
						return null;
					song = this.GetItem (row);
				} else {
					var offset = tableColumn.Identifier == "1" ? 0 : 1;
					var newRow = (row * 2) + offset;
					Console.WriteLine ("New Row:{0}", newRow);
					song = this.GetItem (newRow);
				}
				return song;
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				base.SelectionDidChange (notification);
				var cell = TableView.SelectedCell;
				Console.WriteLine (cell);
			}

			AlbumDetailsSongCell selectedCell;

			public void CellTapped (AlbumDetailsSongCell cell)
			{
				selectedCell?.Deselect ();
				selectedCell = cell;
				selectedCell?.Selected ();
			}
		}
	}
}
