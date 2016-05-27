using System;
using AppKit;
using Foundation;
using MusicPlayer.Models;

namespace MusicPlayer
{
	[RegisterAttribute("AlbumDetailsSongCell")]
	public class AlbumDetailsSongCell : NSTableCellView
	{
		NSTextField trackField;
		TwoLabelView textField;
		NSColorView backgroudView;
		public const string Key = "AlbumDetailsSongCell";
//		public AlbumDetailsSongCell (IntPtr handle) : base (handle)
//		{
//			this.Identifier = Key;
//			AddSubview(artistTextField = new NSTextField {
//				Bezeled = false,
//				DrawsBackground = false,
//				Editable = false,
//				Selectable = false,
//				TextColor = NSColor.LabelColor
//			});
//		}
		public AlbumDetailsSongCell ()
		{
			this.Identifier = Key;
			AddSubview (backgroudView = new NSColorView {
				BackgroundColor = NSColor.ControlLightHighlight,
				Hidden = true,
			});

			AddSubview(trackField = new NSTextField ().StyleAsMainText());
			AddSubview (textField = new TwoLabelView {
				TopLabel = {
					TextColor = NSColor.LabelColor,
				},
				BottomLabel = {
					TextColor = NSColor.SecondaryLabelColor,
				}
			});
			this.AddGestureRecognizer(new NSClickGestureRecognizer(()=>{
				var station = Song;
				if(station == null)
					return;
				ViewModel?.PlayItem(Song);
			}){NumberOfClicksRequired = 2});
			this.AddGestureRecognizer(new NSClickGestureRecognizer(()=>{
				ViewModel?.CellTapped(this);
			}){NumberOfClicksRequired = 1});
		}

		WeakReference _viewModel;
		public MusicPlayer.AlbumDetailView.AlbumDetailsTwinListViewModel ViewModel {
			get {
				return _viewModel?.Target  as MusicPlayer.AlbumDetailView.AlbumDetailsTwinListViewModel;
			}
			set {
				_viewModel = new WeakReference(value);
			}
		}

		WeakReference _song;
		public Song Song {
			get {
				return _song?.Target  as Song;
			}
			set {
				_song = new WeakReference(value);
				Update (value);
			}
		}
		public void Update(Song song)
		{
			trackField.StringValue = song?.Track > 0 ? song.Track.ToString() : "";
			textField.TopLabel.StringValue = song?.Name ?? "";
			textField.BottomLabel.StringValue = song?.Artist ?? "";
		}
		public void Selected()
		{
			backgroudView.Hidden = false;
		}

		public void Deselect()
		{
			backgroudView.Hidden = true;

		}
		public override bool IsFlipped {
			get {
				return true;
			}
		}
		const float trackWidth = 20f;
		const float padding = 5f;
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);

			var bounds = Bounds;
			backgroudView.Frame = bounds;
			trackField.SizeToFit ();
			var frame = trackField.Frame;
			frame.X = padding;
			frame.Y = (bounds.Height - frame.Height) / 2;
			trackField.Frame = frame;

			frame = bounds;
			frame.X = trackWidth + padding * 2;
			frame.Width -= frame.X;
			textField.Frame = frame;
		}
	}
}

